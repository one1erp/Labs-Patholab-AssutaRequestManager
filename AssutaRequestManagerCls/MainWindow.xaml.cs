using EditClientUserCont;
using EditDoctorUserCont;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_DAL_V1;
using Recive_Request.Classes;
using Recive_Request.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;


namespace AssutaRequestManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {

        IExtensionWindowSite2 _ntlsSite;
        LSSERVICEPROVIDERLib.INautilusDBConnection _ntlsConn;
        INautilusServiceProvider _sp;
        DataLayer dal;
        U_SAMPLE_MSG_USER SMU = null;
        bool isChanged = false;
        PHRASE_HEADER InterfaceParams;
        bool isUpdateByID, isDoctorUpdate = false;
        enum UpdateBy
        {
            NotUpdated,
            UpdateByName,
            UpdateByID
        }
        UpdateBy update = UpdateBy.NotUpdated;
        private Dictionary<string, string> _genderDic;
        List<SUPPLIER> doctors;
        List<U_CUSTOMER> customers;
        List<U_CLINIC> clinics;
        //List<string> doctorsFullNames;
        private ListData ListData;
        public event Action SupplierAdded;


        internal void InitializeData(LSSERVICEPROVIDERLib.INautilusDBConnection _ntlsCon, INautilusServiceProvider sp, bool isRightClick, string sampleMsgName)
        {
            try
            {

                dal = new DataLayer();
                _ntlsConn = _ntlsCon;
                dal.Connect(_ntlsConn);
                //_ntlsSite = site;
                InterfaceParams = dal.GetPhraseByName("sample msg status");
                ListData = new ListData(dal);

                _sp = sp;
                _genderDic = dal.GetPhraseByName("Gender").PhraseEntriesDictonary;

                //Get all clinics and convert them to a list
                clinics = dal.GetAll<U_CLINIC>().Where(x => x.NAME.Contains("אסותא")).GroupBy(x => x.U_CLINIC_USER.U_ASSUTA_CLINIC_CODE).Select(x => x.FirstOrDefault()).ToList();
                initializeComboClinic();


                //Get all doctors and convert them to a list
                doctors = dal.GetAll<SUPPLIER>().ToList();
                //foreach(SUPPLIER supplier in doctors)
                //{
                //    doctorsFullNames.Add(supplier.SUPPLIER_USER.U_FIRST_NAME + " " + su

                //}
                initializeComboDoctors();


                //Get all the customers and conver them to a list
                customers = dal.GetAll<U_CUSTOMER>().ToList();
                initializeComboCustomer();


                //Press ENTER to search SMU
                userTextBoxID.KeyDown += new KeyEventHandler(tb_KeyDown);
                findClientButton.Click += new RoutedEventHandler(Find_Button_Click);
                searchIDTextBox.KeyDown += new KeyEventHandler(search_ID_KeyDown);
                searchNameTextBox.KeyDown += new KeyEventHandler(search_Name_KeyDown);

                //if open by right click extention
                if (isRightClick)
                {
                    userTextBoxID.Text = sampleMsgName;
                    Find_Button_Click(new object(), new RoutedEventArgs());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in  InitializeData " + "/n" + ex.Message);
                Patholab_Common.Logger.WriteLogFile(ex);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void initializeComboClinic()
        {
            comboBoxClinics.DisplayMemberPath = "NAME";// clinics;
            //Set the itemSource to show the clinics names
            comboBoxClinics.ItemsSource = clinics;
            //Add event listener when the clinic is changed
            comboBoxClinics.SelectionChanged += new SelectionChangedEventHandler(ComboBoxClinics_SelectedIndexChanged);
        }

        private void initializeComboCustomer()
        {
            comboBoxCustomer.DisplayMemberPath = "NAME";
            comboBoxCustomer.ItemsSource = customers;
            comboBoxCustomer.SelectionChanged += new SelectionChangedEventHandler(ComboBoxCustomer_SelectedIndexChanged);
        }

        private void initializeComboDoctors()
        {
           // comboBoxRefferDR.DisplayMemberPath = "U_LAST_NAME";

            //Set the itemSource to show the doctors names
            comboBoxRefferDR.ItemsSource = doctors;
            comboBoxRefferDR.SelectionChanged += new SelectionChangedEventHandler(ComboBoxRefferDR_SelectedIndexChanged);
          //  comboBoxImpDR.DisplayMemberPath = "U_LAST_NAME";
            //Set the itemSource to show the doctors names
            comboBoxImpDR.ItemsSource = doctors;
            comboBoxImpDR.SelectionChanged += new SelectionChangedEventHandler(ComboBoxImpDR_SelectedIndexChanged);

            comboBoxRefferDRByID.ItemsSource = doctors;
            comboBoxRefferDRByID.SelectionChanged += new SelectionChangedEventHandler(ComboBoxRefferDR_SelectedIndexChanged);
            comboBoxRefferDRByID.DisplayMemberPath = "NAME";

            comboBoxImpDRByID.ItemsSource = doctors;
            comboBoxImpDRByID.SelectionChanged += new SelectionChangedEventHandler(ComboBoxImpDR_SelectedIndexChanged);
            comboBoxImpDRByID.DisplayMemberPath = "NAME";
        }

        private void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                Find_Button_Click(new object(), new RoutedEventArgs());
            }
        }

        private void search_Name_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string firstName, lastName;
            List<CLIENT_USER> clients = new List<CLIENT_USER>();
            if (e.Key == Key.Enter && SMU != null && SMU.U_STATUS == "H")
            {
                string[] nameSplit = searchNameTextBox.Text.Split(' ');
                if (nameSplit.Length == 1)
                {
                    firstName = nameSplit[0].Trim();
                    clients = dal.GetAll<CLIENT_USER>().Where(x => x.CLIENT.CLIENT_USER.U_FIRST_NAME.Contains(firstName) || x.CLIENT.CLIENT_USER.U_LAST_NAME.Contains(firstName)).ToList();
                }
                else
                {
                    firstName = nameSplit[0].Trim();
                    lastName = nameSplit[1].Trim();
                    clients = dal.GetAll<CLIENT_USER>().Where(x => (x.CLIENT.CLIENT_USER.U_FIRST_NAME.Contains(firstName) && x.CLIENT.CLIENT_USER.U_LAST_NAME.Contains(lastName))
                                                                    || (x.CLIENT.CLIENT_USER.U_FIRST_NAME.Contains(lastName) && x.CLIENT.CLIENT_USER.U_LAST_NAME.Contains(firstName))).ToList();
                }
                if (clients.Count() == 0)
                {
                    MessageBox.Show("לא נמצאו מטופלים עם השם שהוכנס");
                }
                nameListBox.ItemsSource = clients;
            }
        }

        private void search_ID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter && SMU != null && SMU.U_STATUS == "H")
            {

                var clients = dal.GetAll<CLIENT_USER>().Where(x => x.CLIENT.NAME.Contains(searchIDTextBox.Text.Trim())).ToList();
                idListBox.ItemsSource = clients;
                idListBox.DisplayMemberPath = "CLIENT.NAME";
                if (clients.Count() == 0)
                {
                    MessageBox.Show("לא נמצאו מטופלים עם מספר תז שהוכנס");
                }
            }
        }

        private void resetComboBoxes()
        {
            comboBoxClinics.SelectedIndex = -1;
            comboBoxCustomer.SelectedIndex = -1;
            comboBoxRefferDR.SelectedIndex = -1;
            comboBoxImpDR.SelectedIndex = -1;
            comboBoxRefferDRByID.SelectedIndex = -1;
            comboBoxImpDRByID.SelectedIndex = -1;

            comboBoxClinics.IsEnabled = false;
            comboBoxCustomer.IsEnabled = false;
            comboBoxImpDR.IsEnabled = false;
            comboBoxImpDRByID.IsEnabled = false;
            comboBoxRefferDR.IsEnabled = false;
            comboBoxRefferDRByID.IsEnabled = false;
        }

        private void resetTextBoxes()
        {
            firstNameRaw.Text = "";
            lastNameRaw.Text = "";
            genderRaw.Text = "";
            IDTypeRaw.Text = "";
            clinicRaw.Text = "";
            customerRaw.Text = "";
            refferingDoctorRaw.Text = "";
            refferingDoctorNbrRaw.Text = "";
            implementingDoctorRaw.Text = "";
            implementingDoctorNbrRaw.Text = "";
            customerRaw.Text = "";
            IdNumberRaw.Text = "";
            birthDateRaw.Text = "";
            birthDateCal.Text = "";
            clinicCodeRaw.Text = "";

            customerCal.Text = "";
            clinicCal.Text = "";
            customerCal.Text = "";
            firstNameCal.Text = "";
            lastNameCal.Text = "";
            genderCal.Text = "";
            IDTypeCal.Text = "";
            refferingDoctorCal.Text = "";
            refferingDoctorNbrCal.Text = "";
            implementingDoctorCal.Text = "";
            implementingDoctorNbrCal.Text = "";
            IdNumberCal.Text = "";

            IdNumberUpdate.Content = "";
            firstNameUpdate.Content = "";
            lastNameUpdate.Content = "";
            genderUpdate.Content = "";
            IDTypeUpdate.Content = "";
            msgStatus.Text = "סטאטוס מסר לא זמין";
            msgDate.Text = "תאריך יצירת מסר לא זמין";
            msgAssutaNumber.Text = "מספר אסותא לא זמין";
            refferingDoctorNbrUpdate.Content = "";
            implementingDoctorNbrUpdate.Content = "";
            birthDateUpdate.Content = "";

            searchNameTextBox.IsEnabled = false;
            searchIDTextBox.IsEnabled = false;
            idListBox.IsEnabled = false;
            nameListBox.IsEnabled = false;


        }

        private void Edit_ImpDoctor_Button_Click(object sender, RoutedEventArgs e)
        {
            SUPPLIER supllier = comboBoxImpDR.SelectedIndex != -1 ? (SUPPLIER)comboBoxImpDR.SelectedItem : null;
            SUPPLIER_USER su = supllier != null ? supllier.SUPPLIER_USER : null;
            if (supllier != null)
            {
                EditDoctorControl editDoctor = new EditDoctorControl(su, _sp);
                editDoctor.DoctorEdited += editDoctor_DoctorEdited;
                editDoctor.ShowDialog();
            }
            else
            {
                MessageBox.Show("לא נבחר רופא");
            }
        }


        private void Edit_RefDoctor_Button_Click(object sender, RoutedEventArgs e)
        {
            SUPPLIER supllier = comboBoxRefferDR.SelectedIndex != -1 ? (SUPPLIER)comboBoxRefferDR.SelectedItem : null;
            SUPPLIER_USER su = supllier != null ? supllier.SUPPLIER_USER : null;
            if (supllier != null)
            {
                EditDoctorControl editDoctor = new EditDoctorControl(su, _sp);
                editDoctor.DoctorEdited += editDoctor_DoctorEdited;
                editDoctor.ShowDialog();
            }
            else
            {
                MessageBox.Show("לא נבחר רופא");
            }

        }

        private void editDoctor_DoctorEdited(SUPPLIER_USER obj)
        {
            int refDrIndex = comboBoxRefferDR.SelectedIndex;
            int impDrIndex = comboBoxImpDR.SelectedIndex;
            comboBoxRefferDR.ItemsSource = null;
            comboBoxImpDR.ItemsSource = null;
            comboBoxRefferDR.ItemsSource = doctors;
            comboBoxImpDR.ItemsSource = doctors;
            comboBoxRefferDR.SelectedIndex = refDrIndex;
            comboBoxImpDR.SelectedIndex = impDrIndex;
            isDoctorUpdate = true;
            fillImpPhy();
            fillRefPhy();
            isDoctorUpdate = false;
        }


        private void Edit_Client_Button_Click(object sender, RoutedEventArgs e)
        {
            CLIENT_USER cu;
            if (update == UpdateBy.UpdateByID)
            {
                cu = (CLIENT_USER)idListBox.SelectedItem;
                CLIENT c = cu.CLIENT;
                //EditClient editClient = new EditClient(c, _genderDic, _sp);
                EditClientControl editClient = new EditClientControl(c, _genderDic, _sp);
                editClient.PatientEdited += editClient_PatientEdited;
                //editClient.PatientEdited += updatPat_PatientEdited;
                editClient.ShowDialog();
            }
            else if (update == UpdateBy.UpdateByName)
            {
                cu = (CLIENT_USER)nameListBox.SelectedItem;
                CLIENT c = cu.CLIENT;
                //EditClient editClient = new EditClient(c, _genderDic, _sp);
                EditClientControl editClient = new EditClientControl(c, _genderDic, _sp);
                editClient.PatientEdited += editClient_PatientEdited;
                editClient.ShowDialog();
            }
            else
            {
                MessageBox.Show("לא נבחר נבדק");
            }
        }

        void editClient_PatientEdited(CLIENT obj)
        {
            updateDetails(obj.CLIENT_USER);
        }

        private void Find_Button_Click(object sender, RoutedEventArgs e)
        {
            resetComboBoxes();
            resetTextBoxes();
            try
            {
                if (userTextBoxID.Text != "")
                {
                    int userID = Int32.Parse(userTextBoxID.Text);
                    SMU = dal.FindBy<U_SAMPLE_MSG_USER>(x => x.U_SAMPLE_MSG_ID == userID).FirstOrDefault();
                    //if (SMU != null)
                    //{
                    //    dal.ReloadEntity(SMU);
                    //}

                    if (SMU == null)
                    {
                        SMU = dal.FindBy<U_SAMPLE_MSG_USER>(x => x.U_REQUEST_NUM == userTextBoxID.Text).FirstOrDefault();

                    }
                    if (SMU == null)
                    {
                        MessageBox.Show("מסר במספר זה אינו קיים");
                        return;
                    }

                    if (InterfaceParams == null || InterfaceParams.PhraseEntriesDictonary == null || InterfaceParams.PhraseEntriesDictonary.ContainsKey(SMU.U_STATUS) == false)
                    {
                        System.Windows.Forms.MessageBox.Show("Test");
                    }

                    msgStatus.Text = InterfaceParams.PhraseEntriesDictonary[SMU.U_STATUS];
                    DateTime dateTime;
                    string date = null;
                    if (SMU.U_CREATED_ON != null)
                    {
                        dateTime = (DateTime)SMU.U_CREATED_ON;
                        date = dateTime.ToShortDateString();
                    }
                    msgDate.Text = "נוצר בתאריך: " + date;

                    msgAssutaNumber.Text = "מספר אסותא: " + SMU.U_REQUEST_NUM;

                    IsEnableControls();



                    //Updating these fields to the proper user's data
                    fillFirstName();
                    fillLastName();
                    fillGender();
                    fillIdType();
                    fillIdNumber();
                    fillErrorsIfNeeded();
                    fillBirthDate();
                    fillClinic();
                    fillCustomer();
                    fillImpPhy();
                    fillRefPhy();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
                Patholab_Common.Logger.WriteLogFile(ex);
            }

            isChanged = false;

        }

        private void fillErrorsIfNeeded()
        {
            if (!string.IsNullOrEmpty(SMU.U_ERRORS))
            {
                errorUpdate.Content = SMU.U_ERRORS;
                errorUpdate.Background = Brushes.Yellow;
                errorUpdate.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                errorUpdate.Content = "אין הערות למסר.";
                errorUpdate.Background = Brushes.Transparent;
                errorUpdate.FlowDirection = FlowDirection.RightToLeft;
            }
        }

        private void IsEnableControls()
        {
            if (SMU.U_STATUS == "H")
            {
                searchNameTextBox.IsEnabled = true;
                searchIDTextBox.IsEnabled = true;
                idListBox.IsEnabled = true;
                nameListBox.IsEnabled = true;

                comboBoxClinics.IsEnabled = true;
                comboBoxCustomer.IsEnabled = true;
                comboBoxImpDR.IsEnabled = true;
                comboBoxImpDRByID.IsEnabled = true;
                comboBoxRefferDR.IsEnabled = true;
                comboBoxRefferDRByID.IsEnabled = true;
            }
        }

        private void fillCustomer()
        {
            if (!string.IsNullOrEmpty(SMU.U_INSTNAME))
            {
                fillTextBox(customerRaw, SMU.U_INSTNAME);
            }
            else
            {
                fillMissingTextBox(customerRaw);
            }
            if (SMU.U_ORDER.U_ORDER_USER.U_CUSTOMER != null && !string.IsNullOrEmpty(SMU.U_ORDER.U_ORDER_USER.U_CUSTOMER1.NAME))
            {
                var comboCustomerItem = comboBoxCustomer.Items.Cast<U_CUSTOMER>().FirstOrDefault(item => item.U_CUSTOMER_ID == SMU.U_ORDER.U_ORDER_USER.U_CUSTOMER);
                comboBoxCustomer.SelectedItem = comboCustomerItem;
                fillTextBox(customerCal, SMU.U_ORDER.U_ORDER_USER.U_CUSTOMER1.NAME);
            }
            else
            {
                fillMissingTextBox(customerCal);
            }
        }

        private void fillRefPhy()
        {
            if (!string.IsNullOrEmpty(SMU.U_REF_PHYSICIAN_NAME))
            {
                fillTextBox(refferingDoctorRaw, SMU.U_REF_PHYSICIAN_NAME);
            }
            else
            {
                fillMissingTextBox(refferingDoctorRaw);
            }
            if (SMU.SUPPLIER1 != null)
            {
                SUPPLIER comboRefDrItem = SMU.SUPPLIER1;//comboBoxImpDR.Items.Cast<SUPPLIER>().FirstOrDefault(item => item.SUPPLIER_USER == SMU.SUPPLIER.SUPPLIER_USER);
                //SUPPLIER_USER comboRefDrItem = comboRefDrItemSupplier.SUPPLIER_USER;
                if (!isDoctorUpdate)
                {
                    comboBoxRefferDR.SelectedItem = comboRefDrItem;
                }

                fillTextBox(refferingDoctorCal, SMU.SUPPLIER1.SUPPLIER_USER.U_LAST_NAME);
                if (!string.IsNullOrEmpty(SMU.SUPPLIER1.SUPPLIER_USER.U_ID_NBR))
                {
                    fillTextBox(refferingDoctorNbrCal, SMU.SUPPLIER1.SUPPLIER_USER.U_ID_NBR);
                }
                else
                {
                    fillMissingTextBox(refferingDoctorNbrCal);

                }
            }
            else
            {
                fillMissingTextBox(refferingDoctorCal);
            }
            if (!string.IsNullOrEmpty(SMU.U_REF_PHYSICIAN_NBR))
            {
                fillTextBox(refferingDoctorNbrRaw, SMU.U_REF_PHYSICIAN_NBR);
            }
            else
            {
                fillMissingTextBox(refferingDoctorNbrRaw);
            }
        }

        private void fillImpPhy()
        {


            if (!string.IsNullOrEmpty(SMU.U_IMP_PHYSICIAN_NAME))
            {
                fillTextBox(implementingDoctorRaw, SMU.U_IMP_PHYSICIAN_NAME);
            }
            else
            {
                fillMissingTextBox(implementingDoctorRaw);
            }

            if (SMU.SUPPLIER != null)
            {
                SUPPLIER comboImpDrItem = SMU.SUPPLIER;//comboBoxImpDR.Items.Cast<SUPPLIER>().FirstOrDefault(item => item.SUPPLIER_USER == SMU.SUPPLIER.SUPPLIER_USER);
                //SUPPLIER_USER comboImpDrItem = comboImpDrItemSupplier.SUPPLIER_USER;
                if (!isDoctorUpdate)
                {
                    comboBoxImpDR.SelectedItem = comboImpDrItem;
                }

                fillTextBox(implementingDoctorCal, SMU.SUPPLIER.SUPPLIER_USER.U_LAST_NAME);
                if (!string.IsNullOrEmpty(SMU.SUPPLIER.SUPPLIER_USER.U_ID_NBR))
                {
                    fillTextBox(implementingDoctorNbrCal, SMU.SUPPLIER.SUPPLIER_USER.U_ID_NBR);
                }
                else
                {
                    fillMissingTextBox(implementingDoctorNbrCal);
                }
            }
            else
            {
                fillMissingTextBox(implementingDoctorCal);
            }
            if (!string.IsNullOrEmpty(SMU.U_IMP_PHYSICIAN_NBR))
            {
                fillTextBox(implementingDoctorNbrRaw, SMU.U_IMP_PHYSICIAN_NBR);
            }
            else
            {
                fillMissingTextBox(implementingDoctorNbrRaw);
            }
        }

        private void fillClinic()
        {
            if (!string.IsNullOrEmpty(SMU.U_CLINIC_UNIT) && !string.IsNullOrEmpty(SMU.U_CLINIC_HOS))
            {
                fillTextBox(clinicRaw, SMU.U_CLINIC_HOS);
                fillTextBox(clinicCodeRaw, SMU.U_CLINIC_UNIT);
            }

            else
            {
                fillMissingTextBox(clinicRaw);
                fillMissingTextBox(clinicCodeRaw);
            }

            if (SMU.U_CLINIC != null)
            {
                U_CLINIC clinic;
                clinic = dal.FindBy<U_CLINIC>(x => x.U_CLINIC_USER.U_ASSUTA_CLINIC_CODE == SMU.U_CLINIC.U_CLINIC_USER.U_ASSUTA_CLINIC_CODE).FirstOrDefault();
                if (!string.IsNullOrEmpty(clinic.NAME))
                {
                    //var comboClinicItem1 = comboBoxClinics.ItemBindingGroup
                    var comboClinicItem = comboBoxClinics.Items.Cast<U_CLINIC>().FirstOrDefault(item => item.U_CLINIC_USER.U_ASSUTA_CLINIC_CODE == clinic.U_CLINIC_USER.U_ASSUTA_CLINIC_CODE);
                    comboBoxClinics.SelectedItem = comboClinicItem;
                    fillTextBox(clinicCal, clinic.NAME);
                }

            }
            else
            {
                fillMissingTextBox(clinicCal);
            }
        }

        private void fillBirthDate()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_BD))
            {
                fillTextBox(birthDateRaw, SMU.U_PATIENT_BD);
            }
            else
            {
                fillMissingTextBox(birthDateRaw);
            }

            if (SMU.CLIENT != null && SMU.CLIENT.CLIENT_USER.U_DATE_OF_BIRTH != null)
            {
                DateTime dateTime = (DateTime)SMU.CLIENT.CLIENT_USER.U_DATE_OF_BIRTH;
                var date = dateTime.ToShortDateString();
                fillTextBox(birthDateCal, date.ToString());
            }
            else
            {
                fillMissingTextBox(birthDateCal);
            }
        }

        private void fillIdNumber()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_NAME))
            {
                fillTextBox(IdNumberRaw, SMU.U_PATIENT_NAME);
            }
            else
            {
                fillMissingTextBox(IdNumberRaw);
            }
            if (SMU.U_CLIENT_ID != null && SMU.U_CLIENT_ID.ToString() != "")
            {
                fillTextBox(IdNumberCal, SMU.CLIENT.NAME);
            }
            else
            {
                fillMissingTextBox(IdNumberCal);
            }
        }

        private void fillIdType()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_ID_TYPE))
            {
                if (SMU.U_PATIENT_ID_TYPE == "0")
                {
                    fillTextBox(IDTypeRaw, "תעודת זהות");
                }
                else
                {
                    fillTextBox(IDTypeRaw, "דרכון");
                }
            }
            else
            {
                fillMissingTextBox(IDTypeRaw);
            }
            if (SMU.CLIENT != null && !string.IsNullOrEmpty(SMU.CLIENT.CLIENT_USER.U_PASSPORT))
            {
                if (SMU.CLIENT.CLIENT_USER.U_PASSPORT == "F")
                {
                    fillTextBox(IDTypeCal, "תעודת זהות");
                }
                else
                {
                    fillTextBox(IDTypeCal, "דרכון");
                }
                IDTypeCal.Foreground = Brushes.Black;
            }
            else
            {
                fillMissingTextBox(IDTypeCal);
            }
        }

        private void fillGender()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_GENDER))
            {
                if (SMU.U_PATIENT_GENDER == "0")
                {
                    fillTextBox(genderRaw, "נקבה");
                }
                else
                {
                    fillTextBox(genderRaw, "זכר");
                }
            }
            else
            {
                fillMissingTextBox(genderRaw);
            }
            if (SMU.CLIENT != null && !string.IsNullOrEmpty(SMU.CLIENT.CLIENT_USER.U_GENDER))
            {
                if (SMU.CLIENT.CLIENT_USER.U_GENDER == "F")
                {
                    fillTextBox(genderCal, "נקבה");
                }
                else
                {
                    fillTextBox(genderCal, "זכר");
                }
            }
            else
            {
                fillMissingTextBox(genderCal);
            }
        }

        private void fillLastName()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_LAST_NAME))
            {
                fillTextBox(lastNameRaw, SMU.U_PATIENT_LAST_NAME);
            }
            else
            {
                fillMissingTextBox(lastNameRaw);
            }
            if (SMU.CLIENT != null && !string.IsNullOrEmpty(SMU.CLIENT.CLIENT_USER.U_LAST_NAME))
            {
                fillTextBox(lastNameCal, SMU.CLIENT.CLIENT_USER.U_LAST_NAME);
            }
            else
            {
                fillMissingTextBox(lastNameCal);
            }
        }

        private void fillFirstName()
        {
            if (!string.IsNullOrEmpty(SMU.U_PATIENT_FIRST_NAME))
            {
                fillTextBox(firstNameRaw, SMU.U_PATIENT_FIRST_NAME);
            }
            else
            {
                fillMissingTextBox(firstNameRaw);
            }
            if (SMU.CLIENT != null && !string.IsNullOrEmpty(SMU.CLIENT.CLIENT_USER.U_FIRST_NAME))
            {
                fillTextBox(firstNameCal, SMU.CLIENT.CLIENT_USER.U_FIRST_NAME);

            }
            else
            {
                fillMissingTextBox(firstNameCal);
            }
        }

        private void fillTextBox(TextBox textBox, string text)
        {
            textBox.Text = text;
            textBox.Foreground = Brushes.Black;
        }

        private void fillMissingTextBox(TextBox textBox)
        {
            textBox.Text = "חסר";
            textBox.Foreground = Brushes.Red;
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {

            AssutaRequestManager.AssutaRequestManagerCls.CloseWindow(isChanged);
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                //Get the selected item from the comboBoxRefferDR
                SUPPLIER selectedItemSupplier = (SUPPLIER)comboBoxRefferDR.SelectedItem;
                SUPPLIER_USER selectedItem = selectedItemSupplier.SUPPLIER_USER;
                if (selectedItem != null)
                {
                    SMU.SUPPLIER1 = dal.FindBy<SUPPLIER>(x => x.SUPPLIER_ID == selectedItem.SUPPLIER.SUPPLIER_ID).FirstOrDefault();
                    SMU.U_REFERRING_PHYSICIAN = SMU.SUPPLIER1.SUPPLIER_ID;
                }

                //Get the selected item from the comboImpDR
                selectedItemSupplier = (SUPPLIER)comboBoxImpDR.SelectedItem;
                selectedItem = selectedItemSupplier.SUPPLIER_USER;
                if (selectedItem != null)
                {
                    SMU.SUPPLIER = dal.FindBy<SUPPLIER>(x => x.SUPPLIER_ID == selectedItem.SUPPLIER.SUPPLIER_ID).FirstOrDefault();
                    SMU.U_IMPLEMENTING_PHYSICIAN = SMU.SUPPLIER.SUPPLIER_ID;
                }

                //Get the selected item from comboClinic
                U_CLINIC clinic = (U_CLINIC)comboBoxClinics.SelectedItem;
                if (clinic != null)
                {

                    SMU.U_CLINIC = dal.FindBy<U_CLINIC>(x => x.U_CLINIC_ID == clinic.U_CLINIC_ID).FirstOrDefault();
                    SMU.U_CLINIC_ID = SMU.U_CLINIC.U_CLINIC_ID;
                }

                //Get the selected item from comboCustomer
                U_CUSTOMER customer = (U_CUSTOMER)comboBoxCustomer.SelectedItem;
                if (customer != null)
                {
                    SMU.U_ORDER.U_ORDER_USER.U_CUSTOMER = customer.U_CUSTOMER_ID;
                    SMU.U_ORDER.U_ORDER_USER.U_PARTS_ID = SMU.U_ORDER.U_ORDER_USER.U_PARTS_ID;
                }

                //Get the selected item from comboClients
                CLIENT_USER item = null;
                if (update == UpdateBy.UpdateByID)
                {
                    item = (CLIENT_USER)idListBox.SelectedItem;
                }
                if (update == UpdateBy.UpdateByName)
                {
                    item = (CLIENT_USER)nameListBox.SelectedItem;

                }
                if (item != null)
                {
                    SMU.CLIENT = item.CLIENT;
                    SMU.U_CLIENT_ID = item.CLIENT_ID;
                    //CLIENT_USER client = dal.FindBy<CLIENT_USER>(x => x.CLIENT.NAME == item.NAME).FirstOrDefault();
                    //if (client != null)
                    //{
                    //    SMU.CLIENT = client.CLIENT;
                    //    SMU.U_CLIENT_ID = client.CLIENT_ID;
                    //}
                }
                string errors = dal.ValidateSampleMsg(SMU);
                SMU.U_ERRORS = errors;
                if (string.IsNullOrEmpty(errors))
                {
                    MessageBox.Show("שינויים נשמרו בהצלחה");
                }
                else
                {
                    MessageBox.Show("שינויים נשמרו אך חסרים נתונים");

                }
                SMU.U_STATUS = "N";
                dal.SaveChanges();
                isChanged = false;
                AssutaRequestManager.AssutaRequestManagerCls.CloseWindow(isChanged);
            }
        }

        private void ComboBoxClients_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                //Get the sender comboBox details
                ComboBox comboBox = (ComboBox)sender;
                //Get the selected item from the comboBox
                CLIENT_USER selectedItem = (CLIENT_USER)comboBox.SelectedItem;
                if (selectedItem == null)
                {
                    return;
                }
                //Set the selected item to be the new clinic of the client
                isChanged = true;
            }
            else
            {

                comboBoxClinics.SelectedIndex = -1;
            }

        }

        private void ComboBoxClinics_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                //Get the sender comboBox details
                ComboBox comboBox = (ComboBox)sender;
                //Get the selected item from the comboBox
                U_CLINIC selectedItem = (U_CLINIC)comboBox.SelectedItem;
                if (selectedItem == null)
                {
                    return;
                }
                //Set the selected item to be the new clinic of the client
                isChanged = true;
            }
            else
            {

                comboBoxClinics.SelectedIndex = -1;
            }

        }

        private void ComboBoxRefferDR_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                //Get the sender comboBox details
                ComboBox comboBox = (ComboBox)sender;
                //Get the selected item from the comboBox
                SUPPLIER selectedItemSupplier = (SUPPLIER)comboBox.SelectedItem;
                if (selectedItemSupplier == null)
                {
                    return;
                }
                isChanged = true;
                SUPPLIER_USER selectedItem = selectedItemSupplier.SUPPLIER_USER;
                refferingDoctorNbrUpdate.Content = !string.IsNullOrEmpty(selectedItem.U_ID_NBR) ? selectedItem.U_ID_NBR : "לא קיימת תז";
                comboBoxRefferDRByID.SelectedIndex = comboBox.SelectedIndex;
                comboBoxRefferDR.SelectedIndex = comboBox.SelectedIndex;
            }
            else
            {
                comboBoxRefferDR.SelectedIndex = -1;
                comboBoxRefferDRByID.SelectedIndex = -1;
            }
        }

        private void ComboBoxImpDR_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                //Get the sender comboBox details
                ComboBox comboBox = (ComboBox)sender;
                //Get the selected item from the comboBox
                SUPPLIER selectedItemSupplier = (SUPPLIER)comboBox.SelectedItem;
                if (selectedItemSupplier == null)
                {
                    return;
                }
                isChanged = true;
                SUPPLIER_USER selectedItem = selectedItemSupplier.SUPPLIER_USER;
                implementingDoctorNbrUpdate.Content = !string.IsNullOrEmpty(selectedItem.U_ID_NBR) ? selectedItem.U_ID_NBR : "לא קיימת תז";
                comboBoxImpDRByID.SelectedIndex = comboBox.SelectedIndex;
                comboBoxImpDR.SelectedIndex = comboBox.SelectedIndex;
            }
            else
            {
                comboBoxImpDR.SelectedIndex = -1;
                comboBoxImpDRByID.SelectedIndex = -1;
            }

        }

        public void ComboBoxCustomer_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SMU != null && SMU.U_STATUS == "H")
            {
                ComboBox comboBox = (ComboBox)sender;
                U_CUSTOMER selectedItem = (U_CUSTOMER)comboBox.SelectedItem;
                if (selectedItem == null)
                {
                    return;
                }
                isChanged = true;
            }
            else
            {
                comboBoxCustomer.SelectedIndex = -1;
            }
        }

        public void updateDetails(CLIENT_USER selectedItem)
        {

            firstNameUpdate.Content = !string.IsNullOrEmpty(selectedItem.U_FIRST_NAME) ? selectedItem.U_FIRST_NAME : "חסר";
            lastNameUpdate.Content = !string.IsNullOrEmpty(selectedItem.U_LAST_NAME) ? selectedItem.U_LAST_NAME : "חסר";
            IdNumberUpdate.Content = !string.IsNullOrEmpty(selectedItem.CLIENT.NAME) ? selectedItem.CLIENT.NAME : "חסר";
            genderUpdate.Content = !string.IsNullOrEmpty(selectedItem.U_GENDER) ? selectedItem.U_GENDER : "חסר";
            DateTime dateTime;
            string date = null;
            if (selectedItem.U_DATE_OF_BIRTH != null)
            {
                dateTime = (DateTime)selectedItem.U_DATE_OF_BIRTH;
                date = dateTime.ToShortDateString();
            }

            birthDateUpdate.Content = !string.IsNullOrEmpty(date) ? date : "חסר";
            if (selectedItem.U_PASSPORT != null)
            {
                if (selectedItem.U_PASSPORT == "F")
                {
                    IDTypeUpdate.Content = "תעודת זהות";
                }
                if (selectedItem.U_PASSPORT == "T")
                {
                    IDTypeUpdate.Content = "דרכון";
                }
            }
            else
            {
                IDTypeUpdate.Content = "חסר";
            }
        }

        public CLIENT_USER findClientUserFromComboBox(String fullName)
        {
            if (fullName == null)
            {
                return null;
            }
            String[] nameSplit = fullName.Split(' ');
            String firstName = nameSplit[0], lastName = nameSplit[1];
            CLIENT_USER selectedItem = dal.FindBy<CLIENT_USER>(x => x.CLIENT.CLIENT_USER.U_FIRST_NAME == firstName && x.CLIENT.CLIENT_USER.U_LAST_NAME == lastName).FirstOrDefault();

            return selectedItem;
        }

        private void idListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;

            // This is your selected item
            CLIENT_USER item = (CLIENT_USER)list.SelectedItem;
            //CLIENT_USER clientUser = dal.FindBy<CLIENT_USER>(x => x.CLIENT.NAME == item.NAME).FirstOrDefault();
            updateDetails(item);
            update = UpdateBy.UpdateByID;
        }

        private void nameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;

            // This is your selected item
            //CLIENT item = (CLIENT)list.SelectedItem;
            CLIENT_USER clientUser = (CLIENT_USER)list.SelectedItem;  //dal.FindBy<CLIENT_USER>(x => x.CLIENT.NAME == item.NAME).FirstOrDefault();
            updateDetails(clientUser);
            update = UpdateBy.UpdateByName;
        }

        private void buttonAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (ListData.Suppliers == null)
            {
                ListData.LoadSuppliers();
            }

            AddSupplier ns = new AddSupplier(dal, ListData, _sp);
            ns.SupplierAdded += ns_SupplierAdded;
            ns.ShowDialog();
            //Get all doctors and convert them to a list
            doctors = dal.GetAll<SUPPLIER>().ToList();
            initializeComboDoctors();
            //fillRefPhy();
            //fillImpPhy();

        }


        private void ns_SupplierAdded()
        {
            if (SupplierAdded != null)
                SupplierAdded();
        }


    }
}
