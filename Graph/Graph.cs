using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Graph
{
	public interface IGraph<T>
	{
		IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
	}

	public class Graph<T> : IGraph<T>
	{
		private readonly Dictionary<T, IEnumerable<T>> _nodesConnections;

		public Graph(IEnumerable<ILink<T>> links)
		{
			_nodesConnections = links.GroupBy(link => link.Source)
									 .ToDictionary(link => link.Key, link => link.Select(connection => connection.Target));

		}

		public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
		{
			var possiblePaths = new List<IEnumerable<T>>();

			_nodesConnections.TryGetValue(source, out IEnumerable<T> sourceConnections);

			foreach (var sourceConnection in sourceConnections)
			{
				var actualPath = new List<T>() { source };
				var sourceConnectionPath = SearchTarget(sourceConnection, target, actualPath);

				if (sourceConnectionPath.Count() > 0)
					possiblePaths.Add(sourceConnectionPath);
			}

			return possiblePaths.ToObservable();
		}

		private IList<T> SearchTarget(T source, T target, IList<T> actualPath)
		{
			if (HasNodeBeenVisited(source, actualPath))
				return null;

			actualPath.Add(source);

			if (source.Equals(target))
				return actualPath;

			if (_nodesConnections.TryGetValue(source, out IEnumerable<T> sourceConnections))
			{
				foreach (var sourceConnection in sourceConnections)
				{
					var sourceConnectionPath = SearchTarget(sourceConnection, target, actualPath);

					if (sourceConnectionPath != null)
						return sourceConnectionPath;
				}
			}

			return null;
		}

		private bool HasNodeBeenVisited(T source, IList<T> actualPath)
			=> actualPath.Where(e => e.Equals(source)).Any();
	}
}
