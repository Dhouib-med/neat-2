namespace calculations
{

	public class Connection
	{

		private Node from;
		private Node to;

		private double weight;
		private bool enabled = true;

		public Connection(Node from, Node to)
		{
			this.from = from;
			this.to = to;
		}

		public virtual Node From
		{
			get
			{
				return from;
			}
			set
			{
				this.from = value;
			}
		}


		public virtual Node To
		{
			get
			{
				return to;
			}
			set
			{
				this.to = value;
			}
		}


		public virtual double Weight
		{
			get
			{
				return weight;
			}
			set
			{
				this.weight = value;
			}
		}


		public virtual bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				this.enabled = value;
			}
		}


	}

}