/*
 * Copyright 2019, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using SRP;
using System;
using System.IO;
using System.Threading;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Bluetooth;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// Helper class to perform the bluetooth authentication.
	/// </summary>
	class BluetoothAuthentication
	{
		// Constants.
		private static readonly int TIMEOUT_AUTH = 20000;

		private static readonly string API_USERNAME = "apiservice";

		private static readonly string ERROR_AUTH = "Error performing authentication";
		private static readonly string ERROR_AUTH_EXTENDED = ERROR_AUTH + " > {0}";
		private static readonly string ERROR_RESPONSE_NOT_RECEIVED = "Server response not received.";
		private static readonly string ERROR_WRITING = "Error writing in the communication interface.";
		private static readonly string ERROR_BAD_PROOF = "Bad proof of key.";

		private static readonly int LENGTH_SALT = 4;
		private static readonly int LENGTH_EPHEMERAL = 128;
		private static readonly int LENGTH_SESSION_PROOF = 32;
		private static readonly int LENGTH_NONCE = 12;

		// Variables.
		private readonly AbstractXBeeDevice device;
		private readonly string password;

		private object unlockLock = new object();

		private SrpPhase expectedPhase;

		private BluetoothUnlockResponsePacket unlockResponse;

		/// <summary>
		/// Class constructor. Instantiates a new <seealso cref="BluetoothAuthentication"/> with the 
		/// given parameters.
		/// </summary>
		/// <param name="device">XBee device.</param>
		/// <param name="password">Bluetooth password.</param>
		/// <seealso cref="XBeeDevice"/>
		public BluetoothAuthentication(AbstractXBeeDevice device, string password)
		{
			this.device = device;
			this.password = password;
		}

		// Properties.
		/// <summary>
		/// The session key.
		/// </summary>
		public byte[] Key { get; private set; }

		/// <summary>
		/// The TX nonce of the SRP authentication process.
		/// </summary>
		public byte[] TxNonce { get; private set; }

		/// <summary>
		/// The RX nonce of the SRP authentication process.
		/// </summary>
		public byte[] RxNonce { get; private set; }

		/// <summary>
		/// Starts the bluetooth authentication with the XBee device.
		/// </summary>
		/// <exception cref="BluetoothAuthenticationException">If there is any error performing the 
		/// bluetooth authentication.</exception>
		/// <exception cref="InterfaceNotOpenException">If the connection interface is not open.</exception>
		/// <exception cref="XBeeException">If there is any error in the communication process.</exception>
		public void Authenticate()
		{
			// Check connection.
			if (!device.IsOpen)
				throw new InterfaceNotOpenException();
			
			device.PacketReceived += ReceiveBLEPacket;

			try
			{
				User user = new User(API_USERNAME, password);

				// Phase 1.
				byte[] clientEphemeral = user.StartAuthentication();
				expectedPhase = SrpPhase.PHASE_2;
				unlockResponse = null;
				device.SendPacketAsync(new BluetoothUnlockPacket(SrpPhase.PHASE_1, clientEphemeral));
				lock (unlockLock)
				{
					Monitor.Wait(unlockLock, TIMEOUT_AUTH);
				}
				CheckResponsePacket();

				// Phase 2.
				int index = 0;
				byte[] salt = new byte[LENGTH_SALT];
				Array.Copy(unlockResponse.Data, index, salt, 0, salt.Length);
				index += LENGTH_SALT;
				byte[] serverEphemeral = new byte[LENGTH_EPHEMERAL];
				Array.Copy(unlockResponse.Data, index, serverEphemeral, 0, serverEphemeral.Length);

				// Phase 3.
				byte[] clientSessionProof = user.ProcessChallenge(salt, serverEphemeral);
				expectedPhase = SrpPhase.PHASE_4;
				unlockResponse = null;
				device.SendPacketAsync(new BluetoothUnlockPacket(SrpPhase.PHASE_3, clientSessionProof));
				lock (unlockLock)
				{
					Monitor.Wait(unlockLock, TIMEOUT_AUTH);
				}
				CheckResponsePacket();

				// Phase 4.
				index = 0;
				byte[] serverSessionProof = new byte[LENGTH_SESSION_PROOF];
				Array.Copy(unlockResponse.Data, index, serverSessionProof, 0, serverSessionProof.Length);
				index += LENGTH_SESSION_PROOF;
				TxNonce = new byte[LENGTH_NONCE];
				Array.Copy(unlockResponse.Data, index, TxNonce, 0, TxNonce.Length);
				index += LENGTH_NONCE;
				RxNonce = new byte[LENGTH_NONCE];
				Array.Copy(unlockResponse.Data, index, RxNonce, 0, RxNonce.Length);

				user.VerifySession(serverSessionProof);

				if (!user.authenticated)
					throw new BluetoothAuthenticationException(string.Format(ERROR_AUTH_EXTENDED, ERROR_BAD_PROOF));

				// Save the sesion key.
				Key = user.SessionKey;
			}
			catch (IOException e)
			{
				throw new XBeeException(ERROR_WRITING, e);
			}
			finally
			{
				device.PacketReceived -= ReceiveBLEPacket;
			}
		}

		/// <summary>
		/// Checks the unlock response packet and throws the appropriate exception in case of error.
		/// </summary>
		/// <exception cref="BluetoothAuthenticationException">If the unlock response is <c>null</c> 
		/// or if the SRP Phase value of the unlock response is <see cref="SrpPhase.UNKNOWN"/></exception>
		private void CheckResponsePacket()
		{
			if (unlockResponse == null)
				throw new BluetoothAuthenticationException(string.Format(ERROR_AUTH_EXTENDED, ERROR_RESPONSE_NOT_RECEIVED));
			else if (unlockResponse.SrpPhase == SrpPhase.UNKNOWN)
				throw new BluetoothAuthenticationException(string.Format(ERROR_AUTH_EXTENDED, unlockResponse.SrpError.GetName()));
		}

		/// <summary>
		/// Callback called after a <see cref="BluetoothUnlockResponsePacket"/> is received and the 
		/// corresponding Packet Received event has been fired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">The Packet Received event.</param>
		/// <seealso cref="PacketReceivedEventArgs"/>
		private void ReceiveBLEPacket(object sender, PacketReceivedEventArgs e)
		{
			if (!(e.ReceivedPacket is XBeeAPIPacket) || ((XBeeAPIPacket)e.ReceivedPacket).FrameType != APIFrameType.BLE_UNLOCK_RESPONSE)
				return;

			BluetoothUnlockResponsePacket response = (BluetoothUnlockResponsePacket)e.ReceivedPacket;

			// Check if the packet contains the expected phase or an error.
			if (response.SrpPhase != SrpPhase.UNKNOWN && response.SrpPhase != expectedPhase)
				return;

			unlockResponse = response;

			// Continue execution by notifying the lock object.
			lock (unlockLock)
			{
				Monitor.Pulse(unlockLock);
			}
		}
	}
}
