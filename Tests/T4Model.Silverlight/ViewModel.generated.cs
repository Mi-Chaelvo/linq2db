﻿//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/igor-tkachev/t4models).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace T4Model.Silverlight
{
	public partial class ViewModel : INotifyPropertyChanged
	{
		#region NotifiedProp1 : double

		private double _notifiedProp1;
		public  double  NotifiedProp1
		{
			get { return _notifiedProp1; }
			set
			{
				if (_notifiedProp1 != value)
				{
					BeforeNotifiedProp1Changed(value);
					_notifiedProp1 = value;
					AfterNotifiedProp1Changed();

					OnNotifiedProp1Changed();
					OnNotifiedBrush1Changed();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeNotifiedProp1Changed(double newValue);
		partial void AfterNotifiedProp1Changed ();

		public const string NameOfNotifiedProp1 = "NotifiedProp1";

		private static readonly PropertyChangedEventArgs _notifiedProp1ChangedEventArgs = new PropertyChangedEventArgs(NameOfNotifiedProp1);

		private void OnNotifiedProp1Changed()
		{
			OnPropertyChanged(_notifiedProp1ChangedEventArgs);
		}

		#endregion

		#endregion

		#region NotifiedProp2 : int

		private int _notifiedProp2 = 500;
		public  int  NotifiedProp2
		{
			get { return _notifiedProp2; }
			set
			{
				if (_notifiedProp2 != value)
				{
					BeforeNotifiedProp2Changed(value);
					_notifiedProp2 = value;
					AfterNotifiedProp2Changed();

					OnNotifiedProp1Changed();
					OnNotifiedProp2Changed();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeNotifiedProp2Changed(int newValue);
		partial void AfterNotifiedProp2Changed ();

		public const string NameOfNotifiedProp2 = "NotifiedProp2";

		private static readonly PropertyChangedEventArgs _notifiedProp2ChangedEventArgs = new PropertyChangedEventArgs(NameOfNotifiedProp2);

		private void OnNotifiedProp2Changed()
		{
			OnPropertyChanged(_notifiedProp2ChangedEventArgs);
		}

		#endregion

		#endregion

		#region NotifiedBrush1 : Brush

		public Brush NotifiedBrush1
		{
			get { return GetBrush(); }
		}

		#region INotifyPropertyChanged support

		public const string NameOfNotifiedBrush1 = "NotifiedBrush1";

		private static readonly PropertyChangedEventArgs _notifiedBrush1ChangedEventArgs = new PropertyChangedEventArgs(NameOfNotifiedBrush1);

		private void OnNotifiedBrush1Changed()
		{
			OnPropertyChanged(_notifiedBrush1ChangedEventArgs);
		}

		#endregion

		#endregion

		#region INotifyPropertyChanged support

#if !SILVERLIGHT
		[field : NonSerialized]
#endif
		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
#if SILVERLIGHT
				if (System.Windows.Deployment.Current.Dispatcher.CheckAccess())
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				else
					System.Windows.Deployment.Current.Dispatcher.BeginInvoke(
						() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
#else
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
#endif
			}
		}

		protected void OnPropertyChanged(PropertyChangedEventArgs arg)
		{
			if (PropertyChanged != null)
			{
#if SILVERLIGHT
				if (System.Windows.Deployment.Current.Dispatcher.CheckAccess())
					PropertyChanged(this, arg);
				else
					System.Windows.Deployment.Current.Dispatcher.BeginInvoke(
						() => PropertyChanged(this, arg));
#else
				PropertyChanged(this, arg);
#endif
			}
		}

		#endregion
	}
}
