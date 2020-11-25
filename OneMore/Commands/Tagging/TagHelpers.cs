﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	internal static class TagHelpers
	{

		public static Dictionary<string, string> FetchRecentTags(OneNote.Scope scope, int poolSize)
		{
			using (var one = new OneNote())
			{
				// builds a hierarchy of all notebooks with notebook/section/page nodes
				// and each Page has a Meta of tags

				var scopeId = string.Empty;
				switch (scope)
				{
					case OneNote.Scope.Sections: scopeId = one.CurrentNotebookId; break;
					case OneNote.Scope.Pages: scopeId = one.CurrentSectionId; break;
				}

				var root = one.SearchMeta(scopeId, Page.TaggingMetaName);

				var ns = root.GetNamespaceOfPrefix("one");
				var pages = root.Descendants(ns + "Page")
					.OrderByDescending(e => e.Attribute("lastModifiedTime").Value);

				var tags = new Dictionary<string, string>();

				var count = 0;
				foreach (var page in pages)
				{
					var meta = page.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == Page.TaggingMetaName);

					if (meta != null)
					{
						// tags are entered in user's language so split on their separator
						var parts = meta.Attribute("content").Value.Split(
							new string[] { AddIn.Culture.TextInfo.ListSeparator },
							StringSplitOptions.RemoveEmptyEntries);

						foreach (var part in parts)
						{
							var p = part.Trim();
							var key = p.ToLower();
							if (!tags.ContainsKey(key))
							{
								tags.Add(key, p);

								count++;
								if (count >= poolSize) break;
							}
						}
					}

					if (count >= poolSize) break;
				}

				return tags;
			}
		}
	}
}
