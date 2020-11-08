using System.Collections.Generic;

namespace neat
{

	using Genome = genome.Genome;
	public class Species
	{

		private data_structures.RandomHashSet<Client> clients = new data_structures.RandomHashSet<Client>();
		private Client representative;
		private double score;

		public Species(Client representative)
		{
			this.representative = representative;
			this.representative.Species = this;
			clients.add(representative);
		}

		public virtual bool put(Client client)
		{
			if (client.distance(representative) < representative.Genome.Neat.CP)
			{
				client.Species = this;
				clients.add(client);
				return true;
			}
			return false;
		}

		public virtual void force_put(Client client)
		{
			client.Species = this;
			clients.add(client);
		}

		public virtual void goExtinct()
		{
			foreach (Client c in clients.Data)
			{
				c.Species = null;
			}
		}

		public virtual void evaluate_score()
		{
			double v = 0;
			foreach (Client c in clients.Data)
			{
				v += c.Score;
			}
			score = v / clients.size();
		}

		public virtual void reset()
		{
			representative = clients.random_element();
			foreach (Client c in clients.Data)
			{
				c.Species = null;
			}
			clients.clear();

			clients.add(representative);
			representative.Species = this;
			score = 0;
		}

		public virtual void kill(double percentage)
		{   
			clients.Data.Sort((x,y)=>x.CompareTo(y));

			double amount = percentage * this.clients.size();
			for (int i = 0;i < amount; i++)
			{
				Client client = clients.get(0);
				clients.get(0).Species = null;
				clients.remove(0);



			}
		}

	

		public virtual Genome breed()
		{
			Client c1 = clients.random_element();
			Client c2 = clients.random_element();

			if (c1.Score > c2.Score)
			{
				return Genome.crossOver(c1.Genome, c2.Genome);
			}
			return Genome.crossOver(c2.Genome, c1.Genome);
		}

		public virtual int size()
		{
			return clients.size();
		}

		public virtual data_structures.RandomHashSet<Client> Clients
		{
			get
			{
				return clients;
			}
		}

		public virtual Client Representative
		{
			get
			{
				return representative;
			}
		}

		public virtual double Score
		{
			get
			{
				return score;
			}
		}
	}

}