using System;

namespace genome
{
	using Calculator = calculations.Calculator;
	using Connection = calculations.Connection;
	using Neat = neat.Neat;

	public class Genome
	{

		private data_structures.RandomHashSet<ConnectionGene> connections = new data_structures.RandomHashSet<ConnectionGene>();
		private data_structures.RandomHashSet<NodeGene> nodes = new data_structures.RandomHashSet<NodeGene>();

		private Neat neat;


		public Genome(Neat neat)
		{
			this.neat = neat;
		}


		
		public virtual double distance(Genome g2)
		{

			Genome g1 = this;
			int highest_innovation_gene1 = 0;
			if (g1.Connections.size() != 0)
			{
				highest_innovation_gene1 = g1.Connections.get(g1.Connections.size() - 1).Innovation_number;
			}

			int highest_innovation_gene2 = 0;
			if (g2.Connections.size() != 0)
			{
				highest_innovation_gene2 = g2.Connections.get(g2.Connections.size() - 1).Innovation_number;
			}

			if (highest_innovation_gene1 < highest_innovation_gene2)
			{
				Genome g = g1;
				g1 = g2;
				g2 = g;
			}

			int index_g1 = 0;
			int index_g2 = 0;

			int disjoint = 0;
			int excess = 0;
			double weight_diff = 0;
			int similar = 0;


			while (index_g1 < g1.Connections.size() && index_g2 < g2.Connections.size())
			{

				ConnectionGene gene1 = g1.Connections.get(index_g1);
				ConnectionGene gene2 = g2.Connections.get(index_g2);

				int in1 = gene1.Innovation_number;
				int in2 = gene2.Innovation_number;

				if (in1 == in2)
				{
				
					similar++;
					weight_diff += Math.Abs(gene1.Weight - gene2.Weight);
					index_g1++;
					index_g2++;
				}
				else if (in1 > in2)
				{
					disjoint++;
					index_g2++;
				}
				else
				{
				
					disjoint++;
					index_g1++;
				}
			}

			weight_diff /= Math.Max(1,similar);
			excess = g1.Connections.size() - index_g1;

			double N = Math.Max(g1.Connections.size(), g2.Connections.size());
			if (N < 20)
			{
				N = 1;
			}

			return neat.C1 * disjoint / N + neat.C2 * excess / N + neat.C3 * weight_diff / N;

		}

		
		public static Genome crossOver(Genome g1, Genome g2)
		{
			Neat neat = g1.Neat;

			Genome genome = neat.empty_genome();

			int index_g1 = 0;
			int index_g2 = 0;

			while (index_g1 < g1.Connections.size() && index_g2 < g2.Connections.size())
			{

				ConnectionGene gene1 = g1.Connections.get(index_g1);
				ConnectionGene gene2 = g2.Connections.get(index_g2);

				int in1 = gene1.Innovation_number;
				int in2 = gene2.Innovation_number;

				if (in1 == in2)
				{
					if (GlobalRandom.NextDouble > 0.5)
					{
						genome.Connections.add(Neat.getConnection(gene1));
					}
					else
					{
						genome.Connections.add(Neat.getConnection(gene2));
					}
					index_g1++;
					index_g2++;
				}
				else if (in1 > in2)
				{
					
					index_g2++;
				}
				else
				{
			
					genome.Connections.add(Neat.getConnection(gene1));
					index_g1++;
				}
			}

			while (index_g1 < g1.Connections.size())
			{
				ConnectionGene gene1 = g1.Connections.get(index_g1);
				genome.Connections.add(Neat.getConnection(gene1));
				index_g1++;
			}

			foreach (ConnectionGene c in genome.Connections.Data)
			{
				genome.Nodes.add(c.From);
				genome.Nodes.add(c.To);
			}

			return genome;
		}




		public virtual void mutate()
		{
			if (neat.PROBABILITY_MUTATE_LINK > GlobalRandom.NextDouble)
			{
				mutate_link();
			}
			if (neat.PROBABILITY_MUTATE_NODE > GlobalRandom.NextDouble)
			{
				mutate_node();
			}
			if (neat.PROBABILITY_MUTATE_WEIGHT_SHIFT > GlobalRandom.NextDouble)
			{
				mutate_weight_shift();
			}
			if (neat.PROBABILITY_MUTATE_WEIGHT_RANDOM > GlobalRandom.NextDouble)
			{
				mutate_weight_random();
			}
			if (neat.PROBABILITY_MUTATE_TOGGLE_LINK > GlobalRandom.NextDouble)
			{
				mutate_link_toggle();
			}
		}

		public virtual void mutate_link()
		{

			for (int i = 0; i < 100; i++)
			{

				NodeGene a = nodes.random_element();
				NodeGene b = nodes.random_element();

				if (a == null || b == null)
				{
					continue;
				}
				if (a.X == b.X)
				{
					continue;
				}

				ConnectionGene con;
				if (a.X < b.X)
				{
					con = new ConnectionGene(a,b);
				}
				else
				{
					con = new ConnectionGene(b,a);
				}

				if (connections.contains(con))
				{
					continue;
				}

				con = neat.getConnection(con.From, con.To);
				con.Weight = (GlobalRandom.NextDouble * 2 - 1) * neat.WEIGHT_RANDOM_STRENGTH;
                data_structures.RandomHashSet<Gene> hi=new data_structures.RandomHashSet<Gene>();
				hi.add_sorted(connections,con);
				return;
			}
		}

		public virtual void mutate_node()
		{
			ConnectionGene con = connections.random_element();
			if (con == null)
			{
				return;
			}

			NodeGene from = con.From;
			NodeGene to = con.To;

			int replaceIndex = neat.getReplaceIndex(from,to);
			NodeGene middle;
			if (replaceIndex == 0)
			{
				middle = neat.Node;
				middle.X = (from.X + to.X) / 2;
				middle.Y = (from.Y + to.Y) / 2 + GlobalRandom.NextDouble * 0.1 - 0.05;
				neat.setReplaceIndex(from, to, middle.Innovation_number);
			}
			else
			{
				middle = neat.getNode(replaceIndex);
			}

			ConnectionGene con1 = neat.getConnection(from, middle);
			ConnectionGene con2 = neat.getConnection(middle, to);

			con1.Weight = 1;
			con2.Weight = con.Weight;
			con2.Enabled = con.Enabled;

			connections.remove(con);
			connections.add(con1);
			connections.add(con2);

			nodes.add(middle);
		}

		public virtual void mutate_weight_shift()
		{
			ConnectionGene con = connections.random_element();
			if (con != null)
			{
				con.Weight = con.Weight + (GlobalRandom.NextDouble * 2 - 1) * neat.WEIGHT_SHIFT_STRENGTH;
			}
		}

		public virtual void mutate_weight_random()
		{
			ConnectionGene con = connections.random_element();
			if (con != null)
			{
				con.Weight = (GlobalRandom.NextDouble * 2 - 1) * neat.WEIGHT_RANDOM_STRENGTH;
			}
		}

		public virtual void mutate_link_toggle()
		{
			ConnectionGene con = connections.random_element();
			if (con != null)
			{
				con.Enabled = !con.Enabled;
			}
		}

		public virtual data_structures.RandomHashSet<ConnectionGene> Connections
		{
			get
			{
				return connections;
			}
		}

		public virtual data_structures.RandomHashSet<NodeGene> Nodes
		{
			get
			{
				return nodes;
			}
		}

		public virtual Neat Neat
		{
			get
			{
				return neat;
			}
		}



	}

}