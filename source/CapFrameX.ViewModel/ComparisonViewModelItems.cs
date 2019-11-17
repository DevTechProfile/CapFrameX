﻿using CapFrameX.Contracts.Data;
using CapFrameX.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CapFrameX.ViewModel
{
	public partial class ComparisonViewModel
	{
		private void AddComparisonItem(IFileRecordInfo recordInfo)
		{
			var stopwatchData = new Stopwatch();
			stopwatchData.Start();
			var comparisonRecordInfo = GetComparisonRecordInfoFromFileRecordInfo(recordInfo);
			var wrappedComparisonRecordInfo = GetWrappedRecordInfo(comparisonRecordInfo);

			// Insert into list (sorted)
			SetMetrics(wrappedComparisonRecordInfo);
			InsertComparisonRecordsSorted(wrappedComparisonRecordInfo);

			HasComparisonItems = ComparisonRecords.Any();

			// Manage game name header
			HasUniqueGameNames = GetHasUniqueGameNames();
			CurrentGameName = comparisonRecordInfo.Game;

			// Update height of bar chart control here
			UpdateBarChartHeight();
			UpdateCuttingParameter();
			stopwatchData.Stop();
			Console.WriteLine("Duration data part: " + stopwatchData.ElapsedMilliseconds);

			var stopwatchChart = new Stopwatch();
			stopwatchChart.Start();
			//Draw charts and performance parameter
			UpdateCharts();
			stopwatchChart.Stop();
			Console.WriteLine("Duration chart part: " + stopwatchChart.ElapsedMilliseconds);
		}

		private void SetMetrics(ComparisonRecordInfoWrapper wrappedComparisonRecordInfo)
		{
			double startTime = FirstSeconds;
			double lastFrameStart = wrappedComparisonRecordInfo.WrappedRecordInfo.Session.FrameStart.Last();
			double endTime = LastSeconds > lastFrameStart ? lastFrameStart : lastFrameStart - LastSeconds;
			var frametimeTimeWindow = wrappedComparisonRecordInfo.WrappedRecordInfo.Session.GetFrametimeTimeWindow(startTime, endTime, ERemoveOutlierMethod.None);
			double GeMetricValue(IList<double> sequence, EMetric metric) =>
					_frametimeStatisticProvider.GetFpsMetricValue(sequence, metric);

			wrappedComparisonRecordInfo.WrappedRecordInfo.FirstMetric
				= GeMetricValue(frametimeTimeWindow, EMetric.Average);

			wrappedComparisonRecordInfo.WrappedRecordInfo.SecondMetric
				= GeMetricValue(frametimeTimeWindow, SelectSecondaryMetric);

			// ToDo: implement third metric
		}

		private void InsertComparisonRecordsSorted(ComparisonRecordInfoWrapper wrappedComparisonRecordInfo)
		{
			if (!ComparisonRecords.Any())
			{
				ComparisonRecords.Add(wrappedComparisonRecordInfo);
				return;
			}

			var list = new List<ComparisonRecordInfoWrapper>(ComparisonRecords)
			{
				wrappedComparisonRecordInfo
			};

			var orderedList = IsSortModeAscending ? list.OrderBy(x => x.WrappedRecordInfo.FirstMetric).ToList() :
				list.OrderByDescending(x => x.WrappedRecordInfo.FirstMetric).ToList();

			var index = orderedList.IndexOf(wrappedComparisonRecordInfo);
			ComparisonRecords.Insert(index, wrappedComparisonRecordInfo);
		}

		public void RemoveComparisonItem(ComparisonRecordInfoWrapper wrappedComparisonRecordInfo)
		{
			_comparisonColorManager.FreeColor(wrappedComparisonRecordInfo.Color);
			ComparisonRecords.Remove(wrappedComparisonRecordInfo);

			HasComparisonItems = ComparisonRecords.Any();
			UpdateCuttingParameter();
			UpdateCharts();
			BarChartHeight = 40 + (2 * BarChartMaxRowHeight + 12) * ComparisonRecords.Count;

			// Manage game name header		
			HasUniqueGameNames = GetHasUniqueGameNames();
			if (HasUniqueGameNames)
				CurrentGameName = ComparisonRecords.First().WrappedRecordInfo.Game;

			ComparisonModel.InvalidatePlot(true);
		}

		private bool GetHasUniqueGameNames()
		{
			if (!ComparisonRecords.Any())
				return false;

			var firstName = ComparisonRecords.First().WrappedRecordInfo.Game;

			return !ComparisonRecords.Any(record => record.WrappedRecordInfo.Game != firstName);
		}

		public void RemoveAllComparisonItems(bool manageVisibility = true, bool resetSortMode = false)
		{
			if (resetSortMode)
			{
				_comparisonColorManager.FreeAllColors();
			}

			ComparisonRecords.Clear();
			UpdateCharts();

			if (resetSortMode)
				IsSortModeAscending = false;

			if (manageVisibility)
			{
				HasComparisonItems = false;
			}

			// Manage game name header
			HasUniqueGameNames = false;
			CurrentGameName = string.Empty;

			RemainingRecordingTime = "0.0 s";
			UpdateCuttingParameter();
			ComparisonModel.InvalidatePlot(true);
		}

		public void SortComparisonItems()
		{
			if (!ComparisonRecords.Any())
				return;

			IEnumerable<ComparisonRecordInfoWrapper> comparisonRecordList = null;
			if (IsSortModeAscending)
				comparisonRecordList = ComparisonRecords.ToList()
					.Select(info => info.Clone())
					.OrderBy(info => info.WrappedRecordInfo.FirstMetric);
			else
				comparisonRecordList = ComparisonRecords.ToList()
					.Select(info => info.Clone())
					.OrderByDescending(info => info.WrappedRecordInfo.FirstMetric);

			// RemoveAllComparisonItems(false, false);
			ComparisonRecords.Clear();

			foreach (var item in comparisonRecordList)
			{
				ComparisonRecords.Add(item);
			}

			//Draw charts and performance parameter
			UpdateCharts();
		}
	}
}
