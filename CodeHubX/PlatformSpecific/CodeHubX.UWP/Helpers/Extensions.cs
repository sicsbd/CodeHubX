using HtmlAgilityPack;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CodeHubX.Helpers;

namespace CodeHubX.UWP.Helpers
{
	/// <summary>
	/// A collection of various extension methods
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns the first element of a specific type in the visual tree of a DependencyObject
		/// </summary>
		/// <typeparam name="T">The type of the element to find</typeparam>
		/// <param name="parent">The object that contains the UIElement to find</param>
		public static T FindChild<T>([NotNull] this DependencyObject parent) where T : UIElement
		{
			if (parent is T)
			{
				return parent.To<T>();
			}

			var children = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < children; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (!(child is T))
				{
					var tChild = FindChild<T>(child);
					if (tChild != null)
						return tChild;
				}
				else
					return child as T;
			}

			return null;
		}

		/// <summary>
		/// Scrolls an input ListView to the top, if possible
		/// </summary>
		/// <param name="listView">The target list to scroll</param>
		public static bool ScrollToTheTop([NotNull] this ListView listView)
		{
			var scrollViewer = listView.FindChild<ScrollViewer>();
			if (scrollViewer == null)
				return false;

			scrollViewer.ChangeView(null, 0, null, false);
			return true;
		}

		/// <summary>
		/// Counts the number of items in an IEnumerable sequence
		/// </summary>
		/// <param name="enumerable">The sequence to count</param>
		public static int Count([NotNull] this IEnumerable enumerable)
			=> Enumerable.Count(enumerable.Cast<object>());

		/// <summary>
		/// Waits for a task with the given token
		/// </summary>
		/// <typeparam name="T">The type returned by the task</typeparam>
		/// <param name="task">The task to wait</param>
		/// <param name="token">The cancellation token</param>
		/// <param name="failsafe">If true, all possible exceptions will be handled too</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<T> AsCancellableTask<T>([NotNull] this Task<T> task,
		    CancellationToken token, bool failsafe = false) where T : class
		{
			try
			{
				return await task.ContinueWith(t => t.GetAwaiter().GetResult());
			}
			catch (OperationCanceledException) { return null; }
			catch when (failsafe) { return null; }
		}

		/// <summary>
		/// Gets a sequence of sibling nodes from the input node
		/// </summary>
		/// <param name="node">The source node</param>
		public static IEnumerable<HtmlNode> Siblings([NotNull] this HtmlNode node)
		{
			while (node != null)
			{
				yield return node.NextSibling;
				node = node.NextSibling;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		public static IEnumerable<HtmlNode> DescendantsWithAttribute([NotNull] this HtmlNode node,
		    [NotNull] string tag, [NotNull] string attribute, [NotNull] string value)
			=> node
				.Descendants(tag)?
				.Where(child => child.Attributes?
				.AttributesWithName(attribute)?
				.FirstOrDefault()?.Value?.Equals(value) == true);

		public static Color GetLight(this Color color, double delta)
		{
			var R = (255 - color.R) * delta + color.R;
			var G = (255 - color.G) * delta + color.G;
			var B = (255 - color.B) * delta + color.B;
			byte normalize(double d)
			{
				if (d < 0)
					return 0;

				return d <= 255 ? (byte) d : (byte) 255;
			}

			return Color.FromArgb(color.A, normalize(R), normalize(G), normalize(B));
		}
	}
}