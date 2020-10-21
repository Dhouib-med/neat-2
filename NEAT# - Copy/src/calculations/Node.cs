using System;
using System.Collections.Generic;

namespace calculations
{

	public class Node : IComparable<Node>
	{

		private double x;
		private double output;
		private List<Connection> connections = new List<Connection>();

		public Node(double x)
		{
			this.x = x;
		}

		public virtual void calculate()
		{
			double s = 0;
			foreach (Connection c in connections)
			{
				if (c.Enabled)
				{
					s += c.Weight * c.From.Output;
				}
			}
			output = activation_function(s);
		}

		private double activation_function(double x)
		{
			return 1d / (1 + Math.Exp(-x));
		}

		public virtual double X
		{
			set
			{
				this.x = value;
			}
			get
			{
				return x;
			}
		}

		public virtual double Output
		{
			set
			{
				this.output = value;
			}
			get
			{
				return output;
			}
		}

		public virtual List<Connection> Connections
		{
			set
			{
				this.connections = value;
			}
			get
			{
				return connections;
			}
		}





		public  int CompareTo(Node o)
		{
			if (this.x > o.x)
			{
				return -1;
			}
			if (this.x < o.x)
			{
				return 1;
			}
			return 0;
		}
	}

}