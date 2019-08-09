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

using System;
using System.Collections.Generic;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee category of settings.
	/// </summary>
	public class XBeeCategory : ICloneable
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeCategory"/> with the provided parameters.
		/// </summary>
		/// <param name="name">Name of the category.</param>
		/// <param name="description">Description of the category.</param>
		/// <param name="parentCategory">Parent category.</param>
		/// <param name="ownerFirmware">XBee firmware the category belongs to.</param>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeCategory(string name, string description, XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			Name = name;
			Description = description;
			ParentCategory = parentCategory;
			OwnerFirmware = ownerFirmware;
		}

		// Properties
		/// <summary>
		/// Name of the category.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Description of the category.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Parent category.
		/// </summary>
		public XBeeCategory ParentCategory { get; private set; }

		/// <summary>
		/// XBee firmware the category belongs to.
		/// </summary>
		public XBeeFirmware OwnerFirmware { get; set; }

		/// <summary>
		/// List of settings contained in the category.
		/// </summary>
		public List<AbstractXBeeSetting> Settings { get; set; } = new List<AbstractXBeeSetting>();

		/// <summary>
		/// List of sub-categories contained in the category.
		/// </summary>
		public List<XBeeCategory> Categories { get; set; } = new List<XBeeCategory>();

		/// <summary>
		/// Returns the setting corresponding to the given name from the list of settings.
		/// </summary>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <returns>The setting corresponding to the given name, <c>null</c> if it has not been found.</returns>
		public AbstractXBeeSetting GetSetting(string name)
		{
			foreach (AbstractXBeeSetting setting in Settings)
			{
				if (setting.Name.Equals(name))
					return setting;
			}
			return null;
		}

		/// <summary>
		/// Creates a shallow copy of the XBeeCategory.
		/// </summary>
		/// <returns>The copy of the XBeeCategory</returns>
		public object Clone()
		{
			return MemberwiseClone() as XBeeCategory;
		}

		/// <summary>
		/// Clones and returns the XBee category object.
		/// </summary>
		/// <param name="parentCategory">The parent category where the cloned category should be placed.</param>
		/// <param name="ownerFirmware">The owner firmware of the cloned category.</param>
		/// <returns>The cloned XBee category object.</returns>
		public XBeeCategory CloneCategory(XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			XBeeCategory clonedCategory = (XBeeCategory)Clone();
			clonedCategory.ParentCategory = parentCategory;
			clonedCategory.OwnerFirmware = ownerFirmware;

			// Clone the settings and add them to the cloned category.
			List<AbstractXBeeSetting> clonedSettings = new List<AbstractXBeeSetting>();
			foreach (AbstractXBeeSetting setting in Settings)
				clonedSettings.Add(setting.CloneSetting(clonedCategory, ownerFirmware));
			clonedCategory.Settings = clonedSettings;

			// Clone the categories and add them to the cloned category.
			List<XBeeCategory> clonedCategories = new List<XBeeCategory>();
			foreach (XBeeCategory category in Categories)
				clonedCategories.Add(category.CloneCategory(clonedCategory, ownerFirmware));
			clonedCategory.Categories = clonedCategories;

			return clonedCategory;
		}
	}
}
