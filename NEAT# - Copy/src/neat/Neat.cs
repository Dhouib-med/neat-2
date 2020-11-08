using System;
using System.Collections.Generic;

namespace neat
{

	using ConnectionGene = genome.ConnectionGene;
	using Gene = genome.Gene;
	using Genome = genome.Genome;
	using NodeGene = genome.NodeGene;



	public class Neat
	{

		public static readonly int MAX_NODES = (int)Math.Pow(2,20);


		private double C1_ = 1, C2_ = 1, C3_ = 1;

		private double CP_ = 4;



		private double WEIGHT_SHIFT_STRENGTH_ = 0.1;

		private double WEIGHT_RANDOM_STRENGTH_ = 1;


		private double SURVIVORS_ = 0.7;


		private double PROBABILITY_MUTATE_LINK_ = 0.05;

		private double PROBABILITY_MUTATE_NODE_ = 0.05;

		private double PROBABILITY_MUTATE_WEIGHT_SHIFT_ = 0.1;

		private double PROBABILITY_MUTATE_WEIGHT_RANDOM_ = 0.05;

		private double PROBABILITY_MUTATE_TOGGLE_LINK_ = 0.01;

		private Dictionary<ConnectionGene, ConnectionGene> all_connections = new Dictionary<ConnectionGene, ConnectionGene>();
		private data_structures.RandomHashSet<NodeGene> all_nodes = new data_structures.RandomHashSet<NodeGene>();

		private data_structures.RandomHashSet<Client> clients = new data_structures.RandomHashSet<Client>();
		private data_structures.RandomHashSet<Species> species = new data_structures.RandomHashSet<Species>();

		private int max_clients;
		private int output_size;
		private int input_size;

		public Neat(int input_size, int output_size, int clients)
		{
			this.reset(input_size, output_size, clients);
		}

		public virtual Genome empty_genome()
		{
			Genome g = new Genome(this);
			for (int i = 0; i < input_size + output_size; i++)
			{
				g.Nodes.add(getNode(i + 1));
			}
			return g;
		}
		public virtual void reset(int input_size, int output_size, int clients)
		{
			this.input_size = input_size;
			this.output_size = output_size;
			this.max_clients = clients;

			all_connections.Clear();
			all_nodes.clear();
			this.clients.clear();

			for (int i = 0;i < input_size; i++)
			{
				NodeGene n = Node;
				n.X = 0.1;
				n.Y = (i + 1) / (double)(input_size + 1);
			}

			for (int i = 0; i < output_size; i++)
			{
				NodeGene n = Node;
				n.X = 0.9;
				n.Y = (i + 1) / (double)(output_size + 1);
			}

			for (int i = 0; i < max_clients; i++)
			{
				Client c = new Client();
				c.Genome = empty_genome();
				c.generate_calculator();
				this.clients.add(c);
			}
		}

		public virtual Client getClient(int index)
		{
			return clients.get(index);
		}

		public static ConnectionGene getConnection(ConnectionGene con)
		{
			ConnectionGene c = new ConnectionGene(con.From, con.To);
			c.Innovation_number = con.Innovation_number;
			c.Weight = con.Weight;
			c.Enabled = con.Enabled;
			return c;
		}
		public virtual ConnectionGene getConnection(NodeGene node1, NodeGene node2)
		{
			ConnectionGene connectionGene = new ConnectionGene(node1, node2);

			if (all_connections.ContainsKey(connectionGene))
			{
				connectionGene.Innovation_number = all_connections[connectionGene].Innovation_number;
			}
			else
			{
				connectionGene.Innovation_number = all_connections.Count + 1;
				all_connections[connectionGene] = connectionGene;
			}

			return connectionGene;
		}
		public virtual void setReplaceIndex(NodeGene node1, NodeGene node2, int index)
		{
			all_connections[new ConnectionGene(node1, node2)].ReplaceIndex = index;
		}
		public virtual int getReplaceIndex(NodeGene node1, NodeGene node2)
		{
			ConnectionGene con = new ConnectionGene(node1, node2);
			ConnectionGene data = all_connections[con];
			if (data == null)
			{
				return 0;
			}
			return data.ReplaceIndex;
		}

		public virtual NodeGene Node
		{
			get
			{
				NodeGene n = new NodeGene(all_nodes.size() + 1);
				all_nodes.add(n);
				return n;
			}
		}
		public virtual NodeGene getNode(int id)
		{
			if (id <= all_nodes.size())
			{
				return all_nodes.get(id - 1);
			}
			return Node;
		}

		public virtual void evolve()
		{

			gen_species();
			kill();
			remove_extinct_species();
			reproduce();
			mutate();
			foreach (Client c in clients.Data)
			{
				c.generate_calculator();
			}
		}

		public virtual void printSpecies()
		{
			Console.WriteLine("##########################################");
			foreach (Species s in this.species.Data)
			{
				Console.WriteLine(s + "  " + s.Score + "  " + s.size());
			}
		}

		private void reproduce()
		{
			data_structures.RandomSelector<Species> selector = new data_structures.RandomSelector<Species>();
			foreach (Species s in species.Data)
			{
				selector.add(s, s.Score);
			}

			foreach (Client c in clients.Data)
			{
				if (c.Species == null)
				{
					Species s = selector.random();
					c.Genome = s.breed();
					s.force_put(c);
				}
			}
		}

		public virtual void mutate()
		{
			foreach (Client c in clients.Data)
			{
				c.mutate();
			}
		}

		private void remove_extinct_species()
		{
			for (int i = species.size() - 1; i >= 0; i--)
			{
				if (species.get(i).size() <= 1)
				{
					species.get(i).goExtinct();
					species.remove(i);
				}
			}
		}

		private void gen_species()
		{
			foreach (Species s in species.Data)
			{
				s.reset();
			}

			foreach (Client c in clients.Data)
			{
				if (c.Species != null)
				{
					continue;
				}


				bool found = false;
				foreach (Species s in species.Data)
				{
					if (s.put(c))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					species.add(new Species(c));
				}
			}

			foreach (Species s in species.Data)
			{
				s.evaluate_score();
			}
		}

		private void kill()
		{
			foreach (Species s in species.Data)
			{
				s.kill(1 - SURVIVORS_);
			}
		}


		public static void Main(string[] args)
		{
			Neat neat = new Neat(10,1,1000);

			double[] @in = new double[10];
			for (int i = 0; i < 10; i++)
			{
				@in[i] = GlobalRandom.NextDouble;
			}

			for (int i = 0; i < 100; i++)
			{
				foreach (Client c in neat.clients.Data)
				{
					double score = c.calculate(@in)[0];
					c.Score = score;
				}
				neat.evolve();
				neat.printSpecies();



				 Console.WriteLine(neat.BestClient.Score);
				 neat.printScoreInformation();
			}
				Console.WriteLine();
		}




		public virtual data_structures.RandomHashSet<Client> Clients
		{
			get
			{
				return clients;
			}
			set
			{
				this.clients = value;
			}
		}


		public virtual double CP
		{
			get
			{
				return CP_;
			}
			set
			{
				this.CP_ = value;
			}
		}


		public virtual double C1
		{
			get
			{
				return C1_;
			}
			set
			{
				C1_ = value;
			}
		}

		public virtual double C2
		{
			get
			{
				return C2_;
			}
			set
			{
				C2_ = value;
			}
		}

		public virtual double C3
		{
			get
			{
				return C3_;
			}
			set
			{
				C3_ = value;
			}
		}


		public virtual double WEIGHT_SHIFT_STRENGTH
		{
			get
			{
				return WEIGHT_SHIFT_STRENGTH_;
			}
			set
			{
				WEIGHT_SHIFT_STRENGTH_ = value;
			}
		}

		public virtual double WEIGHT_RANDOM_STRENGTH
		{
			get
			{
				return WEIGHT_RANDOM_STRENGTH_;
			}
			set
			{
				WEIGHT_RANDOM_STRENGTH_ = value;
			}
		}

		public virtual double PROBABILITY_MUTATE_LINK
		{
			get
			{
				return PROBABILITY_MUTATE_LINK_;
			}
			set
			{
				PROBABILITY_MUTATE_LINK_ = value;
			}
		}

		public virtual double PROBABILITY_MUTATE_NODE
		{
			get
			{
				return PROBABILITY_MUTATE_NODE_;
			}
			set
			{
				PROBABILITY_MUTATE_NODE_ = value;
			}
		}

		public virtual double PROBABILITY_MUTATE_WEIGHT_SHIFT
		{
			get
			{
				return PROBABILITY_MUTATE_WEIGHT_SHIFT_;
			}
			set
			{
				PROBABILITY_MUTATE_WEIGHT_SHIFT_ = value;
			}
		}

		public virtual double PROBABILITY_MUTATE_WEIGHT_RANDOM
		{
			get
			{
				return PROBABILITY_MUTATE_WEIGHT_RANDOM_;
			}
			set
			{
				PROBABILITY_MUTATE_WEIGHT_RANDOM_ = value;
			}
		}

		public virtual double PROBABILITY_MUTATE_TOGGLE_LINK
		{
			get
			{
				return PROBABILITY_MUTATE_TOGGLE_LINK_;
			}
			set
			{
				PROBABILITY_MUTATE_TOGGLE_LINK_ = value;
			}
		}

		public virtual int Output_size
		{
			get
			{
				return output_size;
			}
			set
			{
				this.output_size = value;
			}
		}

		public virtual int Input_size
		{
			get
			{
				return input_size;
			}
			set
			{
				this.input_size = value;
			}
		}

		public virtual int Max_clients
		{
			get
			{
				return max_clients;
			}
			set
			{
				this.max_clients = value;
			}
		}








		public virtual double SURVIVORS
		{
			get
			{
				return SURVIVORS_;
			}
			set
			{
				SURVIVORS_ = value;
			}
		}

		public virtual Dictionary<ConnectionGene, ConnectionGene> All_connections
		{
			get
			{
				return all_connections;
			}
			set
			{
				this.all_connections = value;
			}
		}

		public virtual data_structures.RandomHashSet<NodeGene> All_nodes
		{
			get
			{
				return all_nodes;
			}
			set
			{
				this.all_nodes = value;
			}
		}

		public virtual data_structures.RandomHashSet<Species> Species
		{
			get
			{
				return species;
			}
			set
			{
				this.species = value;
			}
		}










		public virtual Client BestClient
		{
			get
			{
				Client best = this.clients.Data[0];
    
				foreach (Client t in this.clients.Data)
				{
					if (t.Score > best.Score)
					{
						best = t;
    
					}
    
				}
				return best;
			}
		}
		public virtual void printScoreInformation()
		{
	Client best = this.clients.Data[0];
			double score = 0;
			foreach (Client t in this.clients.Data)
			{
				if (t.Score > best.Score)
				{
					best = t;

				}
				score = score + t.Score;

			}
			Console.WriteLine("best score  :" + best.Score);
			Console.WriteLine("moy score  :" + score / this.max_clients);




		}



	}

}