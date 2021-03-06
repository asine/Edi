﻿namespace Edi.Core.Models.DocumentTypes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Edi.Core.Interfaces.DocumentTypes;
    using Edi.Core.Utillities;

    /// <summary>
    /// </summary>
    [Export(typeof(IDocumentTypeManager))]
	public class DocumentTypeManager : IDocumentTypeManager
	{
		#region fields
		private readonly SortableObservableCollection<IDocumentType> _mDocumentTypes;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public DocumentTypeManager()
		{
			_mDocumentTypes = new SortableObservableCollection<IDocumentType>(new List<IDocumentType>());
		}
		#endregion constructors

		#region properties
		/// <summary>
		/// Gets a collection of document types that are supported by this application.
		/// </summary>
		public ObservableCollection<IDocumentType> DocumentTypes => _mDocumentTypes;

		#endregion properties

		#region Methods
		/// <summary>
		/// Method can be invoked in PRISM MEF module registration to register a new document (viewmodel)
		/// type and its default file extension. The result of this call is an <seealso cref="IDocumentType"/>
		/// object and a <seealso cref="RegisterDocumentTypeEvent"/> event to inform listers about the new
		/// arrival of the new document type.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="description"></param>
		/// <param name="fileFilterName"></param>
		/// <param name="defaultFilter"></param>
		/// <param name="fileOpenMethod"></param>
		/// <param name="createDocumentMethod"></param>
		/// <param name="t"></param>
		/// <param name="sortPriority"></param>
		/// <returns></returns>
		public IDocumentType RegisterDocumentType(string key,
																							 string description,
																							 string fileFilterName,
																							 string defaultFilter,
																							 FileOpenDelegate fileOpenMethod,
																							 CreateNewDocumentDelegate createDocumentMethod,
																							 Type t,
																							 int sortPriority = 0)
		{
			var newFileType = new DocumentType(key, description, fileFilterName, defaultFilter,
																				 fileOpenMethod, createDocumentMethod,
																				 t, sortPriority);

			_mDocumentTypes.Add(newFileType);
			_mDocumentTypes.Sort(i => i.SortPriority, ListSortDirection.Ascending );

			return newFileType;
		}

		/// <summary>
		/// Finds a document type that can handle a file
		/// with the given file extension eg ".txt" or "txt"
		/// when the original file name was "Readme.txt".
		/// 
		/// Always returns the 1st document type handler that matches the extension.
		/// </summary>
		/// <param name="fileExtension"></param>
		/// <param name="trimPeriod">Determines if an additional '.' character is removed
		/// from the given extension string or not.</param>
		/// <returns></returns>
		public IDocumentType FindDocumentTypeByExtension(string fileExtension,
		                                                 bool trimPeriod = false)
		{
			if (string.IsNullOrEmpty(fileExtension))
				return null;

			if (trimPeriod)
			{
				int idx;

				if ((idx = fileExtension.LastIndexOf(".", StringComparison.Ordinal)) >= 0)
					fileExtension = fileExtension.Substring(idx + 1);
			}

			if (string.IsNullOrEmpty(fileExtension))
				return null;

			var ret = _mDocumentTypes.FirstOrDefault(d => d.DefaultFilter == fileExtension);

			return ret;
		}

		/// <summary>
		/// Find a document type based on its key.
		/// </summary>
		/// <param name="typeOfDoc"></param>
		/// <returns></returns>
		public IDocumentType FindDocumentTypeByKey(string typeOfDoc)
		{
			if (string.IsNullOrEmpty(typeOfDoc))
				return null;

			return _mDocumentTypes.FirstOrDefault(d => d.Key == typeOfDoc);
		}

		/// <summary>
		/// Goes through all file/document type definitions and returns a filter string
		/// object that can be used in conjunction with FileOpen and FileSave dialog filters.
		/// </summary>
		/// <param name="key">Get entries for this viewmodel only,
		/// or all entries if key parameter is not set.</param>
		/// <returns></returns>
		public IFileFilterEntries GetFileFilterEntries(string key = "")
		{
			SortedList<int, IFileFilterEntry> ret = new SortedList<int,IFileFilterEntry>();

			if (_mDocumentTypes != null)
			{
				foreach (var item in _mDocumentTypes)
				{
					if (key == string.Empty || key == item.Key)
					{
						// format filter entry like "Structured Query Language (*.sql) |*.sql"
						var s = new FileFilterEntry($"{item.FileFilterName} (*.{item.DefaultFilter}) |*.{item.DefaultFilter}",
																											item.FileOpenMethod);

						ret.Add(item.SortPriority, s);

						// Add all file sub-filters for this viewmodel class
						item.GetFileFilterEntries(ret, item.FileOpenMethod);
					}
				}
			}

			List<IFileFilterEntry> list = new List<IFileFilterEntry>();

			foreach (var item in ret)
				list.Add(item.Value);

			return new FileFilterEntries(list);
		}
		#endregion Methods
	}
}
