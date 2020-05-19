using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ColorPicker.Behaviors
{
	public class NumberInputBehavior : Behavior<TextBox>
	{
		public bool IntegerOnly
		{
			get => (bool)GetValue(IntegerOnlyProperty);
			set => SetValue(IntegerOnlyProperty, value);
		}

		public static readonly DependencyProperty IntegerOnlyProperty =
			DependencyProperty.Register(nameof(IntegerOnly), typeof(bool), typeof(NumberInputBehavior), new UIPropertyMetadata(false));

		public bool AllowMinus
		{
			get => (bool)GetValue(AllowMinusProperty);
			set => SetValue(AllowMinusProperty, value);
		}

		public static readonly DependencyProperty AllowMinusProperty =
			DependencyProperty.Register(nameof(AllowMinus), typeof(bool), typeof(NumberInputBehavior), new UIPropertyMetadata(true));

		public double ChangeStepF
		{
			get => (double)GetValue(ChangeStepFProperty);
			set => SetValue(ChangeStepFProperty, value);
		}

		public static readonly DependencyProperty ChangeStepFProperty =
			DependencyProperty.Register(nameof(ChangeStepF), typeof(double), typeof(NumberInputBehavior), new UIPropertyMetadata(0.1));

		public int ChangeStepI
		{
			get => (int)GetValue(ChangeStepIProperty);
			set => SetValue(ChangeStepIProperty, value);
		}

		public static readonly DependencyProperty ChangeStepIProperty =
			DependencyProperty.Register(nameof(ChangeStepI), typeof(int), typeof(NumberInputBehavior), new UIPropertyMetadata(1));

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
			AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
			AssociatedObject.KeyDown += AssociatedObject_KeyDown;
			AssociatedObject.GotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
			AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
			AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
			AssociatedObject.GotKeyboardFocus -= AssociatedObject_GotKeyboardFocus;
		}

		void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox textBox = sender as TextBox;

			switch (e.Key)
			{
				case Key.Up:
					{
						double v = double.Parse(textBox.Text);
						v += IntegerOnly ? ChangeStepI : ChangeStepF;
						textBox.Text = v.ToString();

						var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
						bindingExpression.UpdateSource();

						e.Handled = true;
					}
					break;
				case Key.Down:
					{
						double v = double.Parse(textBox.Text);
						v -= IntegerOnly ? ChangeStepI : ChangeStepF;
						textBox.Text = v.ToString();

						var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
						bindingExpression.UpdateSource();

						e.Handled = true;
					}
					break;
			}
		}

		void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			TextBox textBox = sender as TextBox;

			if (e.Text == "-")
			{
				if (AllowMinus == false && textBox.Text.Length == 0)
				{
					e.Handled = true;
					return;
				}

				// It limits for last numeric will be not minus.
				if (textBox.Text.Length > 0 && textBox.Text != textBox.SelectedText)
				{
					e.Handled = true;
					return;
				}
			}

			if (e.Text == ".")
			{
				if (textBox.Text.Length == 0)
				{
					e.Handled = true;
					return;
				}

				if (AllowMinus && textBox.Text.Length == 1 && textBox.Text[0] == '-')
				{
					e.Handled = true;
					return;
				}
			}

			if (textBox.Text == "0" && e.Text == "0")
			{
				e.Handled = true;
				return;
			}

			// limit be only numeric.
			var regex = new Regex(CreatePattern());
			e.Handled = regex.IsMatch(textBox.Text + e.Text);
		}

		private string CreatePattern()
		{
			if (IntegerOnly)
			{
				return AllowMinus ? "[^-0-9]" : "[^0-9]";
			}

			return AllowMinus ? "[^-0-9.0]" : "[^0-9.0]";
		}

		private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			switch(e.Key)
			{
				case Key.Return:
					DecidedThisNumeric(textBox);
					e.Handled = true;
					break;
			}
		}

		private void AssociatedObject_GotKeyboardFocus(object sender, RoutedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (textBox.Text == "0" && textBox.Text.Count() > 1)
			{
				textBox.Text = "";
			}
		}

		private void DecidedThisNumeric(TextBox textBox)
		{
			if (string.IsNullOrEmpty(textBox.Text))
			{
				textBox.Text = "0";
			}

			textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
	}
}
