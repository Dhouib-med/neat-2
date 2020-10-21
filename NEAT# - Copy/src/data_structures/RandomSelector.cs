using System.Collections.Generic;

namespace data_structures
{

	public class RandomSelector<T>
	{

		private List<T> objects = new List<T>();
		private List<double> scores = new List<double>();

		private double total_score = 0;

		public virtual void add(T element, double score)
		{
			objects.Add(element);
			scores.Add(score);
			total_score += score;
		}

		public virtual T random()
		{
			double v = GlobalRandom.NextDouble * total_score;
			double c = 0;
			for (int i = 0; i < objects.Count; i++)
			{
				c += scores[i];
				if (c >= v)
				{
					return objects[i];
				}
			}
			return default(T);
		}

		public virtual void reset()
		{
			objects.Clear();
			scores.Clear();
			total_score = 0;
		}

	}

}