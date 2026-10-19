//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XToolkit.Services.Data.Attributes
{
    /// <summary>
    /// Marks the dictionary or list based class to expand its own properties next to the collection items
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExpandCollectionAttribute : Attribute
    {
        private const string DEFAULT_ITEMS_NODE_NAME = "$items";

        internal string ItemsNodeName { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExpandCollectionAttribute() : this(DEFAULT_ITEMS_NODE_NAME)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemsNodeName">Name of the node which hosts the collection items</param>
        public ExpandCollectionAttribute(string itemsNodeName)
        {
            if (string.IsNullOrEmpty(itemsNodeName))
            {
                throw new ArgumentNullException(nameof(itemsNodeName));
            }

            ItemsNodeName = itemsNodeName;
        }
    }
}
