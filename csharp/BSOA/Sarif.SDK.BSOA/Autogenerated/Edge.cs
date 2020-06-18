// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using BSOA.Model;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    ///  GENERATED: BSOA Entity for 'Edge'
    /// </summary>
    [GeneratedCode("BSOA.Generator", "0.5.0")]
    public partial class Edge : PropertyBagHolder, ISarifNode, IRow
    {
        private EdgeTable _table;
        private int _index;

        public Edge() : this(SarifLogDatabase.Current.Edge)
        { }

        public Edge(SarifLog root) : this(root.Database.Edge)
        { }

        internal Edge(EdgeTable table) : this(table, table.Count)
        {
            table.Add();
            Init();
        }

        internal Edge(EdgeTable table, int index)
        {
            this._table = table;
            this._index = index;
        }

        public Edge(
            string id,
            Message label,
            string sourceNodeId,
            string targetNodeId,
            IDictionary<string, SerializedPropertyInfo> properties
        ) 
            : this(SarifLogDatabase.Current.Edge)
        {
            Id = id;
            Label = label;
            SourceNodeId = sourceNodeId;
            TargetNodeId = targetNodeId;
            Properties = properties;
        }

        public Edge(Edge other) 
            : this(SarifLogDatabase.Current.Edge)
        {
            Id = other.Id;
            Label = other.Label;
            SourceNodeId = other.SourceNodeId;
            TargetNodeId = other.TargetNodeId;
            Properties = other.Properties;
        }

        partial void Init();

        public string Id
        {
            get => _table.Id[_index];
            set => _table.Id[_index] = value;
        }

        public Message Label
        {
            get => _table.Database.Message.Get(_table.Label[_index]);
            set => _table.Label[_index] = _table.Database.Message.LocalIndex(value);
        }

        public string SourceNodeId
        {
            get => _table.SourceNodeId[_index];
            set => _table.SourceNodeId[_index] = value;
        }

        public string TargetNodeId
        {
            get => _table.TargetNodeId[_index];
            set => _table.TargetNodeId[_index] = value;
        }

        internal override IDictionary<string, SerializedPropertyInfo> Properties
        {
            get => _table.Properties[_index];
            set => _table.Properties[_index] = value;
        }

        #region IEquatable<Edge>
        public bool Equals(Edge other)
        {
            if (other == null) { return false; }

            if (this.Id != other.Id) { return false; }
            if (this.Label != other.Label) { return false; }
            if (this.SourceNodeId != other.SourceNodeId) { return false; }
            if (this.TargetNodeId != other.TargetNodeId) { return false; }
            if (this.Properties != other.Properties) { return false; }

            return true;
        }
        #endregion

        #region Object overrides
        public override int GetHashCode()
        {
            int result = 17;

            unchecked
            {
                if (Id != default(string))
                {
                    result = (result * 31) + Id.GetHashCode();
                }

                if (Label != default(Message))
                {
                    result = (result * 31) + Label.GetHashCode();
                }

                if (SourceNodeId != default(string))
                {
                    result = (result * 31) + SourceNodeId.GetHashCode();
                }

                if (TargetNodeId != default(string))
                {
                    result = (result * 31) + TargetNodeId.GetHashCode();
                }

                if (Properties != default(IDictionary<string, SerializedPropertyInfo>))
                {
                    result = (result * 31) + Properties.GetHashCode();
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Edge);
        }

        public static bool operator ==(Edge left, Edge right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
        #endregion

        #region IRow
        ITable IRow.Table => _table;
        int IRow.Index => _index;

        void IRow.Reset(ITable table, int index)
        {
            _table = (EdgeTable)table;
            _index = index;
        }
        #endregion

        #region ISarifNode
        public SarifNodeKind SarifNodeKind => SarifNodeKind.Edge;

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Edge DeepClone()
        {
            return (Edge)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Edge(this);
        }
        #endregion

        public static IEqualityComparer<Edge> ValueComparer => EqualityComparer<Edge>.Default;
        public bool ValueEquals(Edge other) => Equals(other);
        public int ValueGetHashCode() => GetHashCode();
    }
}
