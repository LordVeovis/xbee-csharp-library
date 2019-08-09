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

namespace XBeeLibrary.Core.Utils
{
	public class MXIHardwareVersionDictionary
	{
		private const string A = "0x01";
		private const string B = "0x02";
		private const string C = "0x03";
		private const string D = "0x04";
		private const string E = "0x05";
		private const string F = "0x06";
		private const string G = "0x07";
		private const string H = "0x08";
		private const string I = "0x09";
		private const string J = "0x0A";
		private const string K = "0x0B";
		private const string L = "0x0C";
		private const string M = "0x0D";
		private const string N = "0x0E";
		private const string O = "0x0F";
		private const string P = "0x10";
		private const string Q = "0x11";
		private const string R = "0x12";
		private const string S = "0x13";
		private const string T = "0x14";
		private const string U = "0x15";
		private const string V = "0x16";
		private const string W = "0x17";
		private const string X = "0x18";
		private const string Y = "0x19";
		private const string Z = "0x1A";

		public static string GetDictionaryValue(string key)
		{
			switch (key)
			{
				case "A": return A;
				case "B": return B;
				case "C": return C;
				case "D": return D;
				case "E": return E;
				case "F": return F;
				case "G": return G;
				case "H": return H;
				case "I": return I;
				case "J": return J;
				case "K": return K;
				case "L": return L;
				case "M": return M;
				case "N": return N;
				case "O": return O;
				case "P": return P;
				case "Q": return Q;
				case "R": return R;
				case "S": return S;
				case "T": return T;
				case "U": return U;
				case "V": return V;
				case "W": return W;
				case "X": return X;
				case "Y": return Y;
				case "Z": return Z;
				default: return "";
			}
		}
	}

}
