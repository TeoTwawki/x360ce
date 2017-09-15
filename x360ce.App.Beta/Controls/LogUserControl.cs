﻿using System;
using System.Linq;
using System.Windows.Forms;
using JocysCom.ClassLibrary.ComponentModel;
using System.Collections.Generic;

namespace x360ce.App.Controls
{

	public partial class LogUserControl
	{

		public LogUserControl()
		{
			InitializeComponent();
			if (IsDesignMode) return;
            JocysCom.ClassLibrary.Controls.ControlsHelper.ApplyBorderStyle(LogDataGridView);
            //EngineHelper.EnableDoubleBuffering(LogDataGridView);
            LogDataGridView.AutoGenerateColumns = false;
		}

		public bool IsDesignMode { get { return JocysCom.ClassLibrary.Controls.ControlsHelper.IsDesignMode(this); } }

		public void DebugForm_Load(object sender, EventArgs e)
		{
			if (IsDesignMode) return;
            LogDataGridView.DataSource = LogList;
		}


		#region TabPage: Log

		public void LogDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			LogItem li = (LogItem)LogDataGridView.Rows[e.RowIndex].DataBoundItem;
			var name = LogDataGridView.Columns[e.ColumnIndex].Name;
			if (name == StatusColumn.Name)
			{
				
			}
			else if (name == DelayColumn.Name)
			{
				e.Value = Math.Round(li.Delay.TotalMilliseconds);
			}
			else if (name == DataColumn.Name)
			{
				e.Value = JocysCom.ClassLibrary.Text.Helper.CropLines(li.Message);
			}
		}

		private void LogDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;
			var li = (LogItem)LogDataGridView.Rows[e.RowIndex].DataBoundItem;
			if (li.Message != null)
			{
			}
		}

		public SortableBindingList<LogItem> LogList = new SortableBindingList<LogItem>();

		public void ClearLogButton_Click(object sender, EventArgs e)
		{
			LogList.Clear();
		}

		public void LogSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			while (LogList.Count >= LogSizeNumericUpDown.Value)
			{
				var first = LogList.First();
				LogList.Remove(first);
			}
		}

		long GpsCount = 0;
		long KaCount = 0;
		long FmeCount = 0;

		private LogItem lastItem;
		public void LogListAdd(LogItem e)
		{
			if (IsDisposed || Disposing || !IsHandleCreated) return;
			BeginInvoke((Action)(() =>
			{
				// Calculate time from last inserted item.
				if (lastItem != null)
				{
					e.Delay = e.Date.Subtract(lastItem.Date);
				}
				var allowToAdd = true;
				if (allowToAdd)
				{
					lastItem = e;
					AddGridRow(LogDataGridView, LogList, e, (int)LogSizeNumericUpDown.Value);
				}
			}));
		}

		// Contains number for how many row add/remove actions to perform scroll.
		int LogGridDoScrollCount = 0;
		int LogGridFirstRowIndex;
		bool LogGridScrollUp = true;
		object LogGridLock = new object();

		void AddGridRow<T>(DataGridView grid, IList<T> list, T item, int maxRows)
		{
			lock (LogGridLock)
			{
				LogGridFirstRowIndex = grid.FirstDisplayedScrollingRowIndex;
				int displayedCount = grid.DisplayedRowCount(true);
				bool doScroll;
				if (LogGridScrollUp)
				{
					// If first row is visible.
					doScroll = (LogGridFirstRowIndex == 0);
					while (list.Count >= maxRows)
					{
						if (doScroll) LogGridDoScrollCount++;
						list.RemoveAt(list.Count - 1);
					}
					if (doScroll) LogGridDoScrollCount++;
					list.Insert(0, item);
				}
				else
				{
					int lastVisible = (LogGridFirstRowIndex + displayedCount) - 1;
					int lastIndex = grid.RowCount - 1;
					// If last row is visible.
					doScroll = (lastVisible == lastIndex);
					// Scroll Down.
					while (list.Count >= maxRows)
					{
						if (doScroll) LogGridDoScrollCount++;
						list.RemoveAt(0);
					}
					if (doScroll) LogGridDoScrollCount++;
					list.Add(item);
				}
			}
		}

		/// <summary>
		/// Perform Data GridView Scroll Up or Down.
		/// </summary>
		/// <param name="scrollUp">
		/// Scroll up looks better if rows have different hight.
		/// It is because DataGridView doesn't support partial scrolling and
		/// first row will be always fully visible.
		/// </param>
		void ScrollLogGrid(int rowsChanged)
		{
			lock (LogGridLock)
			{
				var grid = LogDataGridView;
				if (LogGridDoScrollCount > 0)
				{
					grid.FirstDisplayedScrollingRowIndex = LogGridScrollUp
						? 0
						: grid.RowCount - 1;
					LogGridDoScrollCount--;
				}
				else
				{
					var rowIndex = LogGridFirstRowIndex;
					// If scroll up and rows added.
					if (LogGridScrollUp && rowsChanged > 0)
					{
						rowIndex += rowsChanged;
					}
					// If scroll down and rows removed.
					else if (!LogGridScrollUp && rowsChanged < 0)
					{
						rowIndex += rowsChanged;
					}
					rowIndex = Math.Min(rowIndex, grid.RowCount - 1);
					if (rowIndex > -1) grid.FirstDisplayedScrollingRowIndex = rowIndex;
				}
			}
		}

		private void LogDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			ScrollLogGrid(e.RowCount);
		}

		private void LogDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			ScrollLogGrid(-e.RowCount);
		}

		#endregion

	}
}
