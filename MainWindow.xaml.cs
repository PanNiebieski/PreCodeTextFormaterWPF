using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PreCodeTextFormater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int TAB_SIZE = 4;
        public string SelectedCodeInText { get; set; }
        public CodeTitleDataContextModel CodeTitle { get; set; }

        public MainWindow()
        {
            Uri iconUri = new Uri("pack://application:,,,/Icon.ico", UriKind.RelativeOrAbsolute);

            this.Icon = BitmapFrame.Create(iconUri);

            InitializeComponent();

            CodeTitle = new CodeTitleDataContextModel();
            CodeTitle.CodeTitle = "";
            this.DataContext = CodeTitle;
        }




        /// <summary>
        /// Public Code property used to return formatted code to Windows Live Writer or the Clipboard.
        /// </summary>


        public string Code { get; set; }

        private void Button_DeDent_Click(object sender, RoutedEventArgs e)
        {
            bool isFirstLine = true;
            int dedentedAmount = 0;
            int firstLine = 0, lastLine = 0;
            int savedSelectionLength = TextBox_Code.SelectionLength;
            int newSelectionStart = TextBox_Code.SelectionStart;
            int charCount = 0;

            SwapTabsForSpaces();

            string[] lines = ToArray(TextBox_Code.Text);

            GetLineRange(lines, ref firstLine, ref lastLine);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i >= firstLine && i <= lastLine)
                {
                    int leadingCount = GetLeadingWhiteSpaceCount(lines[i]);

                    if (leadingCount >= TAB_SIZE)
                    {
                        lines[i] = lines[i].Substring(TAB_SIZE);
                        dedentedAmount = TAB_SIZE;
                    }
                    else if (leadingCount > 0)
                    {
                        lines[i] = lines[i].Substring(leadingCount);
                        dedentedAmount = leadingCount;
                    }

                    charCount += lines[i].Length + Environment.NewLine.Length;

                    if (isFirstLine)
                    {
                        newSelectionStart = (charCount - (lines[i].Length + Environment.NewLine.Length)) + GetLeadingWhiteSpaceCount(lines[i]);
                        isFirstLine = false;
                    }
                    else
                    {
                        if (savedSelectionLength > dedentedAmount)
                        {
                            savedSelectionLength -= dedentedAmount;
                        }

                    }

                    dedentedAmount = 0;
                }
                else
                {
                    charCount += lines[i].Length + Environment.NewLine.Length;
                }

            }

            TextBox_Code.Text = ToText(lines);
            TextBox_Code.SelectionStart = newSelectionStart;
            TextBox_Code.SelectionLength = savedSelectionLength;
            TextBox_Code.Focus();
        }

        private void Button_InDent_Click(object sender, RoutedEventArgs e)
        {
            bool isFirstLine = true;
            int firstLine = 0, lastLine = 0;
            int savedSelectionLength = TextBox_Code.SelectionLength;
            int newSelectionStart = TextBox_Code.SelectionStart;
            int charCount = 0;

            SwapTabsForSpaces();

            string[] lines = ToArray(TextBox_Code.Text);

            GetLineRange(lines, ref firstLine, ref lastLine);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i >= firstLine && i <= lastLine)
                {
                    lines[i] = " ".PadLeft(TAB_SIZE) + lines[i];
                    charCount += lines[i].Length + Environment.NewLine.Length;
                    if (isFirstLine)
                    {
                        newSelectionStart = (charCount - (lines[i].Length + Environment.NewLine.Length)) + GetLeadingWhiteSpaceCount(lines[i]);
                        isFirstLine = false;
                    }
                    else
                    {
                        savedSelectionLength += TAB_SIZE;
                    }
                }
                else
                {
                    charCount += lines[i].Length + Environment.NewLine.Length;
                }
            }

            TextBox_Code.Text = ToText(lines);
            TextBox_Code.SelectionStart = newSelectionStart;
            TextBox_Code.SelectionLength = savedSelectionLength;
            TextBox_Code.Focus();
        }


        private void TextBox_Code_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                //CaretIndex resets to zero after the insert - so we need to store the value here.
                int position = TextBox_Code.CaretIndex;
                TextBox_Code.Text = TextBox_Code.Text.Insert(position, String.Empty.PadLeft(TAB_SIZE));
                TextBox_Code.CaretIndex = position + TAB_SIZE;
                e.Handled = true;


            }
        }

        private void Button_FixIndentation_Click(object sender, RoutedEventArgs e)
        {
            SwapTabsForSpaces();

            if (TextBox_Code.LineCount > 1)
            {
                string[] lines = ToArray(TextBox_Code.Text);
                int firsLineOffset = GetLeadingWhiteSpaceCount(lines[0]);
                if (firsLineOffset > 0)
                    DeDent(firsLineOffset);

                lines = ToArray(TextBox_Code.Text);
                int lastLineOffset = GetLeadingWhiteSpaceCount(lines[lines.Length - 1]);
                if (lastLineOffset > 0)
                    DeDent(lastLineOffset);
            }

            TextBox_Code.Focus();
        }

        private void SwapTabsForSpaces()
        {
            // swap any tab whitespaces for spaces since amount will always be in spaces.
            TextBox_Code.Text = TextBox_Code.Text.Replace("\t", " ".PadLeft(TAB_SIZE));
        }

        /// <summary>
        /// Reduce the indent of all lines in <see cref="TextBox_Code"/>.
        /// </summary>
        /// <param name="amount"></param>
        private void DeDent(int amount)
        {
            try
            {
                string[] lines = ToArray(TextBox_Code.Text);
                for (int i = 0; i < lines.Length; i++)
                {
                    int currentOffset = GetLeadingWhiteSpaceCount(lines[i]);
                    if (currentOffset >= amount)
                        lines[i] = lines[i].Substring(amount);
                }

                TextBox_Code.Text = ToText(lines);
            }
            catch (Exception ex)
            {
                HandleException("DeDent()", ex);
            }
        }

        private static int GetLeadingWhiteSpaceCount(string input)
        {
            try
            {
                if (input == null)
                    throw new ArgumentNullException("input");

                char[] line = input.ToCharArray();
                int count = 0;
                foreach (char c in line)
                {
                    if (Char.IsWhiteSpace(c))
                        count++;
                    else
                        break;
                }

                return count;
            }
            catch (Exception ex)
            {
                HandleException("GetLeadingWhiteSpaceCount()", ex);
                return 0;
            }
        }

        public static string ToText(string[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                var sb = new StringBuilder(lines.Length * 200);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (i < lines.Length - 1)
                        sb.AppendLine(lines[i]);
                    else
                    {
                        sb.Append(lines[i]);
                    }
                }

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string[] ToArray(string text)
        {
            if (String.IsNullOrEmpty(text) == false)
                return text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            else
            {
                return new string[0];
            }
        }

        private static void HandleException(string member, Exception ex)
        {
            MessageBox.Show(String.Format("We're sorry but an error occurred in the PreCode plugin at {0}. Error message: {1}", member, ex.Message));
        }

        /// <summary>
        /// Helper method to get current line, or line range of current selection
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        private void GetLineRange(string[] lines, ref int first, ref int last)
        {
            try
            {
                bool foundFirst = false;
                bool foundLast = false;
                int startPosition = TextBox_Code.SelectionStart;
                int stopPosition = startPosition + TextBox_Code.SelectionLength;
                int charCount = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    charCount += lines[i].Length + Environment.NewLine.Length; //Add two bytes for line endings
                    if (startPosition < charCount && !foundFirst)
                    {
                        first = i;
                        foundFirst = true;
                    }

                    if (stopPosition < charCount && !foundLast)
                    {
                        last = i;
                        foundLast = true;
                    }

                    if (foundFirst && foundLast)
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleException("GetLineRange()", ex);
            }
        }


        /// <summary>
        /// Button_Ok_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextBox_Code.Text))
            {
                //Clean tabs out
                SwapTabsForSpaces();

                Code = TextBox_Code.Text;

                //Encode
                if (CheckBox_HtmlEncode.IsChecked.HasValue && CheckBox_HtmlEncode.IsChecked.Value)
                    Code = HttpUtility.HtmlEncode(Code);

                //Swap line endings for </br>
                if (CheckBox_LineEndings.IsChecked.HasValue && CheckBox_LineEndings.IsChecked.Value)
                    Code = Code.Replace(Environment.NewLine, "<br />");

                //Code = $"<pre><code class=\"hljs {SelectedCodeInText} \">\n\r{Code}\n\r</code></pre>"
                ;

                if (CheckBox_AddPreCode.IsChecked == false)
                {
                    Clipboard.SetText(Code);
                    if (CheckBox_OpenNotepad.IsChecked == true)
                        NotepadHelper.ShowMessage(Code);
                    return;
                }

                if (CodeTitle.CodeTitle.Length > 1)
                    Code = $"<pre><code data-codetitle=\"{CodeTitle.CodeTitle}\" class=\"hljs {SelectedCodeInText} \">{Code}</code></pre>";
                else
                    Code = $"<pre><code class=\"hljs {SelectedCodeInText} \">{Code}</code></pre>";

                if (CheckBox_AddParagraf.IsChecked == true)
                    Code = $"<p>{Code}</p><p>...</p>";

                Clipboard.SetText(Code);
                if (CheckBox_OpenNotepad.IsChecked == true)
                    NotepadHelper.ShowMessage(Code);

            }


        }

        private void ComboBox_SurroundWith_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var f = e.AddedItems[0] as ComboBoxItem;

            this.SelectedCodeInText = f.Tag.ToString();
        }
    }
}
