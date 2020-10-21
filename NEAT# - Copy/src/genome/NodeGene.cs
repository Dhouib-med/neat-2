namespace genome
{
	public class NodeGene : Gene
	{


		private double x, y;

		public NodeGene(int innovation_number) : base(innovation_number)
		{
		}

		public virtual double X
		{
			get
			{
				return x;
			}
			set
			{
				this.x = value;
			}
		}


		public virtual double Y
		{
			get
			{
				return y;
			}
			set
			{
				this.y = value;
			}
		}


		public override bool Equals(object o)
		{
			if (!(o is NodeGene))
			{
				return false;
			}
			return innovation_number == ((NodeGene) o).Innovation_number;
		}

		public override string ToString()
		{
			return "NodeGene{" +
					"innovation_number=" + innovation_number +
					'}';
		}

		public override int GetHashCode()
		{
			return innovation_number;
		}
	}

}