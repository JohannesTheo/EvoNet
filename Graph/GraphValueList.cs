﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Graph
{
	public class GraphValueList : IGraphValueList
	{
		Color color;
		bool drawPlane = true;
		bool drawGraph = true;
		List<IGraphValue> values;
		double graphAlpha = 1;
		double planeAlpha = 0.5;
		float lineThickness = 3;

		public bool DrawGraph
		{
			get { return drawGraph; }
			set { drawGraph = value; }
		}

		public double GraphAlpha
		{
			get { return graphAlpha; }
			set { graphAlpha = value; }
		}
		public bool DrawPlane
		{
			get { return drawPlane; }
			set { drawPlane = value; }
		}

		public double PlaneAlpha
		{
			get { return planeAlpha; }
			set { planeAlpha = value; }
		}


		public GraphValueList()
		{
			values = new List<IGraphValue>();
			color = System.Drawing.Color.Blue;
		}
		public virtual Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
			}
		}

		public void Sort()
		{
			values.Sort(new GraphValueComparer());
		}



		#region IList<IGraphValue> Member

		public int IndexOf(IGraphValue item)
		{
			return values.IndexOf(item);
		}

		public void Insert(int index, IGraphValue item)
		{
			values.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			values.RemoveAt(index);
		}

        public void RemoveRange(int index, int count)
        {
            values.RemoveRange(index, count);
        }

		public IGraphValue this[int index]
		{
			get
			{
				return values[index];
			}
			set
			{
				values[index] = value;
			}
		}

		#endregion

		#region ICollection<IGraphValue> Member

		public void Add(IGraphValue item)
		{
			values.Add(item);
		}

		public void Clear()
		{
			values.Clear();
		}

		public bool Contains(IGraphValue item)
		{
			return values.Contains(item);
		}

		public void CopyTo(IGraphValue[] array, int arrayIndex)
		{
			values.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return values.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(IGraphValue item)
		{
			return values.Remove(item);
		}

		#endregion

		#region IEnumerable<IGraphValue> Member

		public IEnumerator<IGraphValue> GetEnumerator()
		{
			return values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Member

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return values.GetEnumerator();
		}

		#endregion

		#region IGraphValueList Member


		public float LineThickness
		{
			get
			{
				return lineThickness;
			}
			set
			{
				lineThickness = value;
			}
		}



		#endregion
	} 
}

