using System.Data;
using System.Linq;

namespace Quisumbing_ShortExercise2
{
    public partial class Form1 : Form
    {
        private string currentExpression = "";
        private double memoryValue = 0;
        private bool resultJustDisplayed = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Number_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (lblDisplay.Text == "0" || resultJustDisplayed)
            {
                lblDisplay.Text = btn.Text;
                resultJustDisplayed = false;
            }
            else
            {
                lblDisplay.Text += btn.Text;
            }

            // Auto multiply if previous was ")"
            if (currentExpression.EndsWith(")"))
                lblExpression.Text = currentExpression + "*" + lblDisplay.Text;
            else
                lblExpression.Text = currentExpression + lblDisplay.Text;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        private void Operator_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string op = btn.Text;

            if (op == "×") op = "*";
            if (op == "÷") op = "/";

            if (resultJustDisplayed)
            {
                currentExpression = lblDisplay.Text;
                resultJustDisplayed = false;
            }

            if (currentExpression.Length > 0)
            {
                char lastChar = currentExpression[^1];

                // If last character is operator, replace it
                if ("+-*/".Contains(lastChar))
                {
                    currentExpression = currentExpression.Remove(currentExpression.Length - 1);
                }
                // If last character is ')', display not appended
                else if (lastChar == ')')
                {
                    // do nothing
                }
                // Otherwise append display if not zero
                else if (lblDisplay.Text != "0")
                {
                    currentExpression += lblDisplay.Text;
                }
            }
            else
            {
                currentExpression = lblDisplay.Text;
            }

            currentExpression += op;
            lblExpression.Text = currentExpression;
            lblDisplay.Text = "0";
        }

        private void Equals_Click(object sender, EventArgs e)
        {
            try
            {
                if (resultJustDisplayed)
                    return;

                string expression = currentExpression;

                // Append display only if it was actually entered
                if (!string.IsNullOrEmpty(lblDisplay.Text) &&
                    !(lblDisplay.Text == "0" && currentExpression.EndsWith(")")))
                {
                    char lastChar = expression.Length > 0 ? expression[^1] : '\0';

                    if (expression.Length == 0)
                    {
                        expression = lblDisplay.Text;
                    }
                    else if ("+-*/".Contains(lastChar))
                    {
                        expression += lblDisplay.Text;
                    }
                    else if (lastChar == ')')
                    {
                        // Only multiply if user typed something meaningful
                        if (lblDisplay.Text != "0")
                            expression += "*" + lblDisplay.Text;
                    }
                    else
                    {
                        expression += lblDisplay.Text;
                    }
                }

                // Normalize operators
                expression = expression.Replace("×", "*")
                                       .Replace("÷", "/");

                // Implicit multiplication
                expression = System.Text.RegularExpressions.Regex.Replace(
                    expression,
                    @"\)(\d)",
                    ")*$1"
                );

                expression = System.Text.RegularExpressions.Regex.Replace(
                    expression,
                    @"(\d)\(",
                    "$1*("
                );

                expression = expression.Replace(")(", ")*(");

                // Parenthesis check
                int openCount = expression.Count(c => c == '(');
                int closeCount = expression.Count(c => c == ')');

                if (openCount != closeCount)
                {
                    MessageBox.Show("Unmatched parentheses.");
                    return;
                }

                var result = new DataTable().Compute(expression, null);
                double finalResult = Convert.ToDouble(result);

                if (double.IsInfinity(finalResult))
                {
                    MessageBox.Show("Cannot divide by zero.");
                    Clear_Click(null, null);
                    return;
                }

                lblExpression.Text = expression + " =";
                lblDisplay.Text = finalResult.ToString();

                currentExpression = "";
                resultJustDisplayed = true;
            }
            catch
            {
                MessageBox.Show("Invalid Expression");
                Clear_Click(null, null);
            }
        }

        private void Parenthesis_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Text == "(")
            {
                if (lblDisplay.Text != "0")
                    currentExpression += lblDisplay.Text + "*(";
                else
                    currentExpression += "(";

                lblDisplay.Text = "0";
            }
            else
            {
                if (lblDisplay.Text != "0")
                    currentExpression += lblDisplay.Text + ")";
                else
                    currentExpression += ")";

                lblDisplay.Text = "0";
            }

            lblExpression.Text = currentExpression;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            currentExpression = "";
            lblExpression.Text = "";
            lblDisplay.Text = "0";
            resultJustDisplayed = false;
        }

        private void CE_Click(object sender, EventArgs e)
        {
            lblDisplay.Text = "0";
        }

        private void Del_Click(object sender, EventArgs e)
        {
            if (resultJustDisplayed)
            {
                lblExpression.Text = "";
                resultJustDisplayed = false;
            }

            if (lblDisplay.Text.Length > 1)
                lblDisplay.Text = lblDisplay.Text.Substring(0, lblDisplay.Text.Length - 1);
            else
                lblDisplay.Text = "0";

            lblExpression.Text = currentExpression + lblDisplay.Text;
        }

        private void Decimal_Click(object sender, EventArgs e)
        {
            if (!lblDisplay.Text.Contains("."))
                lblDisplay.Text += ".";
        }

        private void Percent_Click(object sender, EventArgs e)
        {
            try
            {
                double currentValue;

                if (!double.TryParse(lblDisplay.Text, out currentValue))
                    return;

                // No base expression → simple percent
                if (string.IsNullOrEmpty(currentExpression))
                {
                    lblDisplay.Text = (currentValue / 100.0).ToString();
                    resultJustDisplayed = true;
                    return;
                }

                char lastOperator = currentExpression[^1];

                // Remove operator to compute base safely
                string baseExpression = currentExpression.Substring(0, currentExpression.Length - 1);

                int openCount = baseExpression.Count(c => c == '(');
                int closeCount = baseExpression.Count(c => c == ')');

                if (openCount != closeCount)
                {
                    MessageBox.Show("Unmatched parentheses.");
                    return;
                }

                double percentValue;

                if (lastOperator == '+' || lastOperator == '-')
                {
                    var result = new DataTable().Compute(baseExpression, null);
                    double baseValue = Convert.ToDouble(result);

                    percentValue = baseValue * (currentValue / 100.0);
                }
                else if (lastOperator == '*' || lastOperator == '/')
                {
                    percentValue = currentValue / 100.0;
                }
                else
                {
                    percentValue = currentValue / 100.0;
                }

                lblDisplay.Text = percentValue.ToString();
                resultJustDisplayed = false;
            }
            catch
            {
                MessageBox.Show("Invalid percent operation.");
            }
        }

        private void ToggleSign_Click(object sender, EventArgs e)
        {
            double value = double.Parse(lblDisplay.Text);
            value = -value;
            lblDisplay.Text = value.ToString();
        }

        private void Square_Click(object sender, EventArgs e)
        {
            double value = double.Parse(lblDisplay.Text);
            lblDisplay.Text = (value * value).ToString();
            resultJustDisplayed = true;
        }

        private void Sqrt_Click(object sender, EventArgs e)
        {
            double value = double.Parse(lblDisplay.Text);

            if (value >= 0)
            {
                lblDisplay.Text = Math.Sqrt(value).ToString();
                resultJustDisplayed = true;
            }
            else
                MessageBox.Show("Invalid input.");
        }

        private void MPlus_Click(object sender, EventArgs e)
        {
            memoryValue += double.Parse(lblDisplay.Text);
            resultJustDisplayed = true;
        }

        private void MMinus_Click(object sender, EventArgs e)
        {
            memoryValue -= double.Parse(lblDisplay.Text);
            resultJustDisplayed = true;
        }

        private void MR_Click(object sender, EventArgs e)
        {
            lblDisplay.Text = memoryValue.ToString();
            resultJustDisplayed = true;
        }

        private void MC_Click(object sender, EventArgs e)
        {
            memoryValue = 0;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}