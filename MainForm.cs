using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PharmaCare
{
    public class MainForm : Form
    {
        //  Colors
        private static readonly Color BG_DARK = Color.FromArgb(13, 27, 42);
        private static readonly Color BG_MID = Color.FromArgb(17, 34, 51);
        private static readonly Color BG_PANEL = Color.FromArgb(10, 22, 40);
        private static readonly Color ACCENT = Color.FromArgb(0, 229, 160);
        private static readonly Color ACCENT2 = Color.FromArgb(0, 180, 216);
        private static readonly Color ACCENT_WARN = Color.FromArgb(255, 107, 107);
        private static readonly Color ACCENT_YLW = Color.FromArgb(249, 199, 79);
        private static readonly Color TEXT_MAIN = Color.FromArgb(232, 244, 240);
        private static readonly Color TEXT_DIM = Color.FromArgb(107, 143, 168);
        private static readonly Color PILL_BG = Color.FromArgb(26, 47, 69);
        private static readonly Color BORDER = Color.FromArgb(30, 58, 82);

        
        //  State
        
        private List<Patient> _patients;
        private HashSet<string> _firedToday = new HashSet<string>();

        private Dictionary<TreeNode, int> _nodeToPatientIdx = new Dictionary<TreeNode, int>();
        private Dictionary<TreeNode, (int, int)> _nodeToRemKey = new Dictionary<TreeNode, (int, int)>();

        private int? _editPatientIdx = null;
        private (int, int)? _editRemKey = null;

     
        //  Controls — Left Panel
        
        private Panel _patientTab, _reminderTab;
        private Button _tabPatientBtn, _tabReminderBtn;

        private TextBox _txtName, _txtAge, _txtGuardian;
        private Label _lblGuardian;
        private RadioButton _rbAdult, _rbChild;
        private Button _btnAddPatient, _btnCancelPatient;

        private ComboBox _cmbPatient;
        private TextBox _txtMed, _txtTime;
        private RadioButton[] _rbMeal;
        private Button _btnAddReminder, _btnCancelReminder;

        //  Controls — Right Panel
     
        private TreeView _tree;

        //  Timer
        
        private Timer _reminderTimer;

        
        //  Constructor
       
        public MainForm()
        {
            _patients = DataManager.LoadPatients();
            InitializeComponent();
            RefreshTree();
            StartReminderTimer();
        }

       
        //  UI Build
      
        private void InitializeComponent()
        {
            this.Text = "PharmaCare — Patient Reminders";
            this.Size = new Size(876, 660);
            this.MinimumSize = new Size(876, 660);
            this.MaximumSize = new Size(876, 660);
            this.BackColor = BG_DARK;
            this.ForeColor = TEXT_MAIN;
            this.Font = new Font("Courier New", 9.5f);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.FormClosing += (s, e) =>
            {
                AlertManager.StopAlert();
                _reminderTimer?.Stop();
            };

            BuildHeader();
            BuildLeftPanel();
            BuildRightPanel();
        }

        //  Header 
        private void BuildHeader()
        {
            var header = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(860, 90),
                BackColor = BG_DARK
            };
            this.Controls.Add(header);

            // Logo (custom drawn)
            var logoBox = new PictureBox
            {
                Location = new Point(16, 10),
                Size = new Size(70, 70),
                BackColor = BG_DARK
            };
            logoBox.Paint += (s, e) => DrawLogo(e.Graphics, 35, 35, 24);
            header.Controls.Add(logoBox);

            header.Controls.Add(new Label
            {
                Text = "PharmaCare",
                Font = new Font("Courier New", 22f, FontStyle.Bold),
                ForeColor = ACCENT,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(96, 12)
            });

            header.Controls.Add(new Label
            {
                Text = "Patient Reminder System",
                Font = new Font("Courier New", 9f),
                ForeColor = TEXT_DIM,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(99, 52)
            });

            // Separator line
            var sep = new Panel
            {
                Location = new Point(0, 88),
                Size = new Size(876, 2),
                BackColor = ACCENT
            };
            this.Controls.Add(sep);
        }

        //  Left Panel 
        private void BuildLeftPanel()
        {
            var left = new Panel
            {
                Location = new Point(0, 90),
                Size = new Size(420, 540),
                BackColor = BG_MID
            };
            this.Controls.Add(left);

            // Tab buttons row
            var tabRow = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(420, 34),
                BackColor = BG_DARK
            };
            left.Controls.Add(tabRow);

            _tabPatientBtn = MakeTabButton("👤  Add Patient");
            _tabPatientBtn.Location = new Point(0, 0);
            _tabPatientBtn.Size = new Size(210, 34);
            _tabPatientBtn.Click += (s, e) => SwitchTab("patient");
            tabRow.Controls.Add(_tabPatientBtn);

            _tabReminderBtn = MakeTabButton("⏰  Add Reminder");
            _tabReminderBtn.Location = new Point(210, 0);
            _tabReminderBtn.Size = new Size(210, 34);
            _tabReminderBtn.Click += (s, e) => SwitchTab("reminder");
            tabRow.Controls.Add(_tabReminderBtn);

            // Patient Tab
            _patientTab = new Panel
            {
                Location = new Point(0, 34),
                Size = new Size(420, 506),
                BackColor = BG_MID
            };
            left.Controls.Add(_patientTab);
            BuildPatientTab();

            // Reminder Tab
            _reminderTab = new Panel
            {
                Location = new Point(0, 34),
                Size = new Size(420, 506),
                BackColor = BG_MID
            };
            left.Controls.Add(_reminderTab);
            BuildReminderTab();

            SwitchTab("patient");
        }

        private Button MakeTabButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Courier New", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = BG_DARK,
                ForeColor = TEXT_DIM,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void SwitchTab(string key)
        {
            if (key == "patient")
            {
                _patientTab.BringToFront();
                _tabPatientBtn.BackColor = BG_MID;
                _tabPatientBtn.ForeColor = ACCENT;
                _tabReminderBtn.BackColor = BG_DARK;
                _tabReminderBtn.ForeColor = TEXT_DIM;
            }
            else
            {
                _reminderTab.BringToFront();
                _tabReminderBtn.BackColor = BG_MID;
                _tabReminderBtn.ForeColor = ACCENT;
                _tabPatientBtn.BackColor = BG_DARK;
                _tabPatientBtn.ForeColor = TEXT_DIM;
            }
        }

        // ─── Patient Tab 
        private void BuildPatientTab()
        {
            int y = 10;

            AddLabel(_patientTab, "Add / Edit Patient", new Font("Courier New", 14f, FontStyle.Bold), ACCENT, new Point(0, y), true);
            y += 44;

            AddLabel(_patientTab, "Patient Name", null, TEXT_DIM, new Point(22, y));
            y += 18;
            _txtName = AddTextBox(_patientTab, new Point(20, y));
            _txtName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) AddPatient(); };
            y += 36;

            AddLabel(_patientTab, "Age", null, TEXT_DIM, new Point(22, y));
            y += 18;
            _txtAge = AddTextBox(_patientTab, new Point(20, y));
            _txtAge.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) AddPatient(); };
            y += 36;

            AddLabel(_patientTab, "Patient Type", null, TEXT_DIM, new Point(22, y));
            y += 18;

            _rbAdult = new RadioButton
            {
                Text = "Adult",
                Checked = true,
                Location = new Point(22, y),
                ForeColor = TEXT_MAIN,
                BackColor = BG_MID,
                Font = new Font("Courier New", 9f),
                AutoSize = true
            };
            _rbChild = new RadioButton
            {
                Text = "Child (under 12)",
                Location = new Point(100, y),
                ForeColor = TEXT_MAIN,
                BackColor = BG_MID,
                Font = new Font("Courier New", 9f),
                AutoSize = true
            };
            _rbAdult.CheckedChanged += (s, e) => ToggleGuardian();
            _rbChild.CheckedChanged += (s, e) => ToggleGuardian();
            _patientTab.Controls.Add(_rbAdult);
            _patientTab.Controls.Add(_rbChild);
            y += 30;

            _lblGuardian = AddLabel(_patientTab, "Guardian Name", null, TEXT_DIM, new Point(22, y));
            _lblGuardian.Visible = false;
            y += 18;
            _txtGuardian = AddTextBox(_patientTab, new Point(20, y));
            _txtGuardian.Visible = false;
            _txtGuardian.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) AddPatient(); };
            y += 36;

            _btnAddPatient = new Button
            {
                Text = "➕  Add Patient",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                BackColor = ACCENT,
                ForeColor = BG_DARK,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(380, 40),
                Location = new Point(20, y),
                Cursor = Cursors.Hand
            };
            _btnAddPatient.FlatAppearance.BorderSize = 0;
            _btnAddPatient.Click += (s, e) => AddPatient();
            _patientTab.Controls.Add(_btnAddPatient);
            y += 50;

            _btnCancelPatient = new Button
            {
                Text = "❌  Cancel Edit",
                Font = new Font("Courier New", 9f),
                BackColor = ACCENT_WARN,
                ForeColor = TEXT_MAIN,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(380, 32),
                Location = new Point(20, y),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnCancelPatient.FlatAppearance.BorderSize = 0;
            _btnCancelPatient.Click += (s, e) => CancelPatientEdit();
            _patientTab.Controls.Add(_btnCancelPatient);
        }

        private void ToggleGuardian()
        {
            bool isChild = _rbChild.Checked;
            _lblGuardian.Visible = isChild;
            _txtGuardian.Visible = isChild;
        }

        // Reminder Tab 
        private void BuildReminderTab()
        {
            int y = 10;

            AddLabel(_reminderTab, "Add / Edit Reminder", new Font("Courier New", 14f, FontStyle.Bold), ACCENT2, new Point(0, y), true);
            y += 44;

            AddLabel(_reminderTab, "Select Patient", null, TEXT_DIM, new Point(22, y));
            y += 18;
            _cmbPatient = new ComboBox
            {
                Location = new Point(20, y),
                Size = new Size(380, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Courier New", 10f),
                BackColor = PILL_BG,
                ForeColor = TEXT_MAIN
            };
            _reminderTab.Controls.Add(_cmbPatient);
            RefreshCombo();
            y += 36;

            AddLabel(_reminderTab, "Medicine Name", null, TEXT_DIM, new Point(22, y));
            y += 18;
            _txtMed = AddTextBox(_reminderTab, new Point(20, y));
            _txtMed.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) AddReminder(); };
            y += 36;

            AddLabel(_reminderTab, "Time (HH:MM) \"for 24h\" ", null, TEXT_DIM, new Point(22, y));
            y += 18;
            _txtTime = AddTextBox(_reminderTab, new Point(20, y));
            _txtTime.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) AddReminder(); };
            y += 36;

            AddLabel(_reminderTab, "Meal Time", null, TEXT_DIM, new Point(22, y));
            y += 20;

            var mealValues = new[] { MealTime.BeforeMeal, MealTime.AfterMeal, MealTime.WithMeal, MealTime.AnyTime };
            _rbMeal = new RadioButton[mealValues.Length];
            for (int i = 0; i < mealValues.Length; i++)
            {
                _rbMeal[i] = new RadioButton
                {
                    Text = mealValues[i].ToDisplayString(),
                    Tag = mealValues[i],
                    Location = new Point(22, y),
                    ForeColor = TEXT_MAIN,
                    BackColor = BG_MID,
                    Font = new Font("Courier New", 9f),
                    AutoSize = true,
                    Checked = (i == 1)
                };
                _reminderTab.Controls.Add(_rbMeal[i]);
                y += 24;
            }

            _btnAddReminder = new Button
            {
                Text = "⏰  Add Reminder",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                BackColor = ACCENT2,
                ForeColor = BG_DARK,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(380, 40),
                Location = new Point(20, y),
                Cursor = Cursors.Hand
            };
            _btnAddReminder.FlatAppearance.BorderSize = 0;
            _btnAddReminder.Click += (s, e) => AddReminder();
            _reminderTab.Controls.Add(_btnAddReminder);
            y += 50;

            _btnCancelReminder = new Button
            {
                Text = "❌  Cancel Edit",
                Font = new Font("Courier New", 9f),
                BackColor = ACCENT_WARN,
                ForeColor = TEXT_MAIN,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(380, 32),
                Location = new Point(20, y),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnCancelReminder.FlatAppearance.BorderSize = 0;
            _btnCancelReminder.Click += (s, e) => CancelReminderEdit();
            _reminderTab.Controls.Add(_btnCancelReminder);
        }

        // ─── Right Panel ──────────────────────────────
        private void BuildRightPanel()
        {
            var right = new Panel
            {
                Location = new Point(420, 90),
                Size = new Size(440, 540),
                BackColor = BG_PANEL
            };
            this.Controls.Add(right);

            right.Controls.Add(new Label
            {
                Text = "👥  Patients & Reminders",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                ForeColor = ACCENT,
                BackColor = BG_PANEL,
                AutoSize = true,
                Location = new Point(10, 12)
            });

            right.Controls.Add(new Label
            {
                Text = "Double-click or select to Edit | Del key to Delete",
                Font = new Font("Courier New", 8f),
                ForeColor = TEXT_DIM,
                BackColor = BG_PANEL,
                AutoSize = true,
                Location = new Point(10, 36)
            });

            _tree = new TreeView
            {
                Location = new Point(10, 58),
                Size = new Size(418, 400),
                BackColor = PILL_BG,
                ForeColor = TEXT_MAIN,
                Font = new Font("Courier New", 9f),
                BorderStyle = BorderStyle.None,
                ItemHeight = 26,
                ShowLines = true,
                ShowPlusMinus = true,
                FullRowSelect = true,
                HideSelection = false,
                DrawMode = TreeViewDrawMode.OwnerDrawText
            };
            _tree.DrawNode += TreeDrawNode;
            _tree.DoubleClick += (s, e) => EditSelected();
            _tree.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) DeleteSelected(); };
            right.Controls.Add(_tree);

            // Button row
            int bx = 10;
            var btnEdit = new Button
            {
                Text = "✏️  Edit",
                Font = new Font("Courier New", 9f),
                BackColor = ACCENT2,
                ForeColor = BG_DARK,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 32),
                Location = new Point(bx, 466),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => EditSelected();
            right.Controls.Add(btnEdit);
            bx += 118;

            var btnDel = new Button
            {
                Text = "🗑  Delete",
                Font = new Font("Courier New", 9f),
                BackColor = ACCENT_WARN,
                ForeColor = TEXT_MAIN,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 32),
                Location = new Point(bx, 466),
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) => DeleteSelected();
            right.Controls.Add(btnDel);
            bx += 118;

            var btnRefresh = new Button
            {
                Text = "↻  Refresh",
                Font = new Font("Courier New", 9f),
                BackColor = BORDER,
                ForeColor = ACCENT,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 32),
                Location = new Point(bx, 466),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshTree();
            right.Controls.Add(btnRefresh);
        }

        // Owner-draw TreeView for dark theme
        private void TreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            var bg = selected ? ACCENT : PILL_BG;
            var fg = selected ? BG_DARK : TEXT_MAIN;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            if (!string.IsNullOrEmpty(e.Node.Text))
            {
                TextRenderer.DrawText(
                    e.Graphics, e.Node.Text,
                    _tree.Font, e.Bounds,
                    fg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                );
            }
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────
        private Label AddLabel(Control parent, string text, Font font, Color fg,
                               Point loc, bool centered = false)
        {
            var lbl = new Label
            {
                Text = text,
                Font = font ?? new Font("Courier New", 9f),
                ForeColor = fg,
                BackColor = parent.BackColor,
                AutoSize = true,
                Location = loc
            };
            parent.Controls.Add(lbl);

            if (centered)
                lbl.Location = new Point((parent.Width - lbl.PreferredWidth) / 2, loc.Y);

            return lbl;
        }

        private TextBox AddTextBox(Control parent, Point loc)
        {
            var tb = new TextBox
            {
                Location = loc,
                Size = new Size(380, 26),
                Font = new Font("Courier New", 11f),
                BackColor = PILL_BG,
                ForeColor = TEXT_MAIN,
                BorderStyle = BorderStyle.None
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private void RefreshCombo()
        {
            _cmbPatient.Items.Clear();
            foreach (var p in _patients)
                _cmbPatient.Items.Add(p.Name);
        }

        private void ShowToast(string msg, Color? color = null)
        {
            var c = color ?? ACCENT;
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = c,
                Size = new Size(300, 40),
                TopMost = true,
                StartPosition = FormStartPosition.Manual
            };
            int tx = this.Left + (this.Width - 300) / 2;
            int ty = this.Top + this.Height - 70;
            toast.Location = new Point(tx, ty);

            toast.Controls.Add(new Label
            {
                Text = msg,
                Font = new Font("Courier New", 10f, FontStyle.Bold),
                ForeColor = BG_DARK,
                BackColor = c,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            toast.Show();
            var t = new Timer { Interval = 1800 };
            t.Tick += (s, e) => { t.Stop(); try { toast.Close(); } catch { } };
            t.Start();
        }

        private void DrawLogo(Graphics g, int cx, int cy, int r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(ACCENT, 1.5f))
                g.DrawEllipse(pen, cx - r - 6, cy - r - 6, (r + 6) * 2, (r + 6) * 2);
            using (var brush = new SolidBrush(ACCENT2))
                g.FillRectangle(brush, cx - r, cy - 8, r * 2, 16);
            using (var brush = new SolidBrush(TEXT_MAIN))
            {
                g.FillRectangle(brush, cx - 5, cy - r, 10, r * 2);
                g.FillRectangle(brush, cx - r, cy - 5, r * 2, 10);
            }
            using (var brush = new SolidBrush(ACCENT))
                g.FillEllipse(brush, cx - 5, cy - 5, 10, 10);
        }

        // ─────────────────────────────────────────────
        //  Add / Edit Patient
        // ─────────────────────────────────────────────
        private void AddPatient()
        {
            string name = _txtName.Text.Trim();
            if (!int.TryParse(_txtAge.Text.Trim(), out int age) || age <= 0 || age > 150)
            {
                MessageBox.Show("Age must be a valid number (1–150).", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(name) || name.Length > 100)
            {
                MessageBox.Show("Name is required (max 100 chars).", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Duplicate check
            if (_editPatientIdx == null)
            {
                foreach (var p in _patients)
                    if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show($"Patient '{name}' already exists.", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
            }

            Patient newPat;
            if (_rbChild.Checked)
            {
                string guardian = _txtGuardian.Text.Trim();
                if (string.IsNullOrEmpty(guardian) || guardian.Length > 100)
                {
                    MessageBox.Show("Guardian name is required (max 100 chars).", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                newPat = new PediatricPatient(name, age, guardian);
            }
            else
                newPat = new Patient(name, age);

            if (_editPatientIdx.HasValue)
            {
                var old = _patients[_editPatientIdx.Value];
                newPat.Reminders = old.Reminders;
                newPat.Id = old.Id;
                _patients[_editPatientIdx.Value] = newPat;
                CancelPatientEdit();
                ShowToast($"✔  {name} updated!");
            }
            else
            {
                _patients.Add(newPat);
                ShowToast($"✔  {name} added!");
            }

            DataManager.SavePatients(_patients);
            _txtName.Clear();
            _txtAge.Clear();
            _txtGuardian.Clear();
            RefreshCombo();
            RefreshTree();
        }

        private void CancelPatientEdit()
        {
            _editPatientIdx = null;
            _btnAddPatient.Text = "➕  Add Patient";
            _btnAddPatient.BackColor = ACCENT;
            _btnCancelPatient.Visible = false;
            _txtName.Clear();
            _txtAge.Clear();
            _txtGuardian.Clear();
        }

        // ─────────────────────────────────────────────
        //  Add / Edit Reminder
        // ─────────────────────────────────────────────
        private void AddReminder()
        {
            string patientName = _cmbPatient.Text;
            string medicine = _txtMed.Text.Trim();
            string timeStr = _txtTime.Text.Trim();

            if (string.IsNullOrEmpty(patientName) || string.IsNullOrEmpty(medicine) || string.IsNullOrEmpty(timeStr))
            {
                MessageBox.Show("All fields are required.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (medicine.Length > 100)
            {
                MessageBox.Show("Medicine name too long (max 100 chars).", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!DateTime.TryParseExact(timeStr, "HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out _))
            {
                MessageBox.Show("Time must be HH:MM  e.g. 08:00", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MealTime meal = MealTime.AnyTime;
            foreach (var rb in _rbMeal)
                if (rb.Checked) { meal = (MealTime)rb.Tag; break; }

            for (int pIdx = 0; pIdx < _patients.Count; pIdx++)
            {
                var p = _patients[pIdx];
                if (!p.Name.Equals(patientName, StringComparison.OrdinalIgnoreCase)) continue;

                if (_editRemKey.HasValue)
                {
                    var (pi, ri) = _editRemKey.Value;
                    if (pi < _patients.Count && ri < p.Reminders.Count)
                    {
                        p.Reminders[ri] = new Reminder(medicine, timeStr, meal);
                        CancelReminderEdit();
                        ShowToast("✔  Reminder updated!");
                    }
                }
                else
                {
                    p.AddReminder(new Reminder(medicine, timeStr, meal));
                    ShowToast($"⏰  Reminder set for {patientName}!");
                }
                break;
            }

            DataManager.SavePatients(_patients);
            _txtMed.Clear();
            _txtTime.Clear();
            RefreshTree();
        }

        private void CancelReminderEdit()
        {
            _editRemKey = null;
            _btnAddReminder.Text = "⏰  Add Reminder";
            _btnAddReminder.BackColor = ACCENT2;
            _btnCancelReminder.Visible = false;
            _txtMed.Clear();
            _txtTime.Clear();
        }

        // ─────────────────────────────────────────────
        //  Edit Selected
        // ─────────────────────────────────────────────
        private void EditSelected()
        {
            var node = _tree.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("Please select a patient or reminder to edit.", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_nodeToPatientIdx.TryGetValue(node, out int pIdx))
            {
                // Edit patient
                if (pIdx >= _patients.Count) return;
                var p = _patients[pIdx];

                _editPatientIdx = pIdx;
                _txtName.Text = p.Name;
                _txtAge.Text = p.Age.ToString();

                if (p is PediatricPatient pp)
                {
                    _rbChild.Checked = true;
                    _txtGuardian.Text = pp.Guardian;
                }
                else
                    _rbAdult.Checked = true;

                ToggleGuardian();
                _btnAddPatient.Text = "💾  Save Changes";
                _btnAddPatient.BackColor = ACCENT2;
                _btnCancelPatient.Visible = true;
                SwitchTab("patient");
            }
            else if (_nodeToRemKey.TryGetValue(node, out (int pi, int ri) key))
            {
                // Edit reminder
                var p = _patients[key.pi];
                var r = p.Reminders[key.ri];

                _editRemKey = key;
                _cmbPatient.Text = p.Name;
                _txtMed.Text = r.Medicine;
                _txtTime.Text = r.TimeStr;

                foreach (var rb in _rbMeal)
                    rb.Checked = ((MealTime)rb.Tag == r.MealTime);

                _btnAddReminder.Text = "💾  Save Changes";
                _btnAddReminder.BackColor = ACCENT2;
                _btnCancelReminder.Visible = true;
                SwitchTab("reminder");
            }
        }

        // ─────────────────────────────────────────────
        //  Delete Selected
        // ─────────────────────────────────────────────
        private void DeleteSelected()
        {
            var node = _tree.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("Please select a patient or reminder.", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_nodeToPatientIdx.TryGetValue(node, out int pIdx))
            {
                if (pIdx >= _patients.Count) return;
                string name = _patients[pIdx].Name;
                if (MessageBox.Show($"Delete '{name}' and all their reminders?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _patients.RemoveAt(pIdx);
                    DataManager.SavePatients(_patients);
                    RefreshCombo();
                    RefreshTree();
                    ShowToast("✔  Deleted.", ACCENT_WARN);
                }
            }
            else if (_nodeToRemKey.TryGetValue(node, out (int pi, int ri) key))
            {
                if (key.pi >= _patients.Count) return;
                var p = _patients[key.pi];
                if (key.ri >= p.Reminders.Count) return;
                var r = p.Reminders[key.ri];

                if (MessageBox.Show($"Delete reminder '{r.Medicine}' at {r.TimeStr}?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    p.Reminders.RemoveAt(key.ri);
                    DataManager.SavePatients(_patients);
                    RefreshTree();
                    ShowToast("✔  Deleted.", ACCENT_WARN);
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Refresh Tree
        // ─────────────────────────────────────────────
        public void RefreshTree()
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            _nodeToPatientIdx.Clear();
            _nodeToRemKey.Clear();

            for (int pIdx = 0; pIdx < _patients.Count; pIdx++)
            {
                var p = _patients[pIdx];
                string label = p is PediatricPatient pp2
                    ? $"👶 {p.Name}  ({p.Age}y) — {pp2.Guardian}"
                    : $"👤 {p.Name}  ({p.Age}y)";

                var patNode = new TreeNode(label) { ForeColor = ACCENT };
                _nodeToPatientIdx[patNode] = pIdx;

                for (int rIdx = 0; rIdx < p.Reminders.Count; rIdx++)
                {
                    var r = p.Reminders[rIdx];
                    var remNode = new TreeNode($"  💊 {r.Medicine}  |  {r.TimeStr}  |  {r.MealTime.ToDisplayString()}")
                    {
                        ForeColor = TEXT_MAIN
                    };
                    _nodeToRemKey[remNode] = (pIdx, rIdx);
                    patNode.Nodes.Add(remNode);
                }

                _tree.Nodes.Add(patNode);
                patNode.Expand();
            }

            _tree.EndUpdate();
        }

        // ─────────────────────────────────────────────
        //  Reminder Timer
        // ─────────────────────────────────────────────
        private void StartReminderTimer()
        {
            _reminderTimer = new Timer { Interval = 10_000 };
            _reminderTimer.Tick += CheckReminders;
            _reminderTimer.Start();
        }

        private void CheckReminders(object sender, EventArgs e)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            foreach (var p in _patients)
            {
                foreach (var r in p.Reminders)
                {
                    string key = $"{today}|{p.Id}|{r.Id}";
                    if (_firedToday.Contains(key)) continue;
                    if (!r.IsDueNow()) continue;

                    _firedToday.Add(key);

                    string pName = p.Name;
                    string med = r.Medicine;
                    string mealTm = r.MealTime.ToDisplayString();

                    // Fire notification on UI thread
                    this.BeginInvoke(new Action(() =>
                    {
                        FireNotification(pName, med, mealTm, key);
                    }));
                }
            }
        }

        private void FireNotification(string patientName, string medicine,
                                      string mealTime, string key)
        {
            AlertManager.StartAlert();

            void SnoozeCallback(int delayMs)
            {
                _firedToday.Remove(key);
                var snoozeTimer = new Timer { Interval = delayMs };
                snoozeTimer.Tick += (s, e) =>
                {
                    snoozeTimer.Stop();
                    snoozeTimer.Dispose();
                    FireNotification(patientName, medicine, mealTime, key);
                };
                snoozeTimer.Start();
            }

            var notif = new NotificationForm(patientName, medicine, mealTime, SnoozeCallback);
            notif.Show(this);
        }
    }
}