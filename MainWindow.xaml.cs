using AutosarGuiEditor.Source.Render;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using AutosarGuiEditor.Source.Forms;
using AutosarGuiEditor.Source.Fabrics;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Controllers;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.RteGenerator;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Forms.SystemErrorsForm;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Utility;
using AutosarGuiEditor.Source.Tester;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.Utility.GuiUtility;
using AutosarGuiEditor.Source.RteGenerator.TestGenerator;
using AutosarGuiEditor.Source.Painters.Boundaries;
using AutosarGuiEditor.Source.App.Settings;


namespace AutosarGuiEditor
{ 
    public partial class MainWindow : Window
    {
        AutosarApplication autosarApp = AutosarApplication.GetInstance();
              
        Scene scene;

        ViewPort viewPort;
        public DispatcherTimer workTimer;

        /* Menus */
        SimpleDataTypeMenu simpleDataTypeMenu;
        ComplexDataTypeMenu complexDataTypeMenu;
        SenderReceiverInterfaceController senderReceiverInterfaceController;
        ClientServerInterfaceController clientServerInterfaceController;
        ComponentDefenitionController componentDefenitionViewController;
        OpenSaveController openSaveController;
        AddObjectController addComponentInstancesController;
        ConnectionLineController connectionLineController;
        ChangeViewportScaleController changeViewportScaleController;
        ComponentPropertiesController componentPropertiesController;
        CompositionInstanceController compositionInstanceController;
        ArrayDataTypeController arrayDataController;
        TabHiderHelper tabHideHelper;
        MoveObjectsController moveObjectsController = new MoveObjectsController();
        EnumsMenu enumsMenu;                
        
        public MainWindow()
        {
            InitializeComponent();
            viewPort = new ViewPort((int)ViewPortImage.Width, (int)ViewPortImage.ActualHeight);
            ViewPortImage.Source = viewPort.WriteableBmp;            
            scene = new Scene(viewPort.WriteableBmp);

            simpleDataTypeMenu = new SimpleDataTypeMenu(autosarApp,
                SimpleDataTypeMenu_NameTextBox,
                SimpleDataTypeMenu_DataTypeComboBox,
                SimpleDataTypeMenu_MaxValueTextBox,
                SimpleDataTypeMenu_MinValueTextBox,
                SimpleDataTypeMenu_ApplyButton
                );

            complexDataTypeMenu = new ComplexDataTypeMenu(AutosarTree, ComplexDataTypeGridView, ComplexDataType_NameTextBox);
            enumsMenu = new EnumsMenu(AutosarTree, Enums_GridView, EnumDataType_NameEdit);
            senderReceiverInterfaceController = new SenderReceiverInterfaceController(AutosarTree, SenderReceiver_GridView, SenderReceiver_NameTextBox);
            clientServerInterfaceController = new ClientServerInterfaceController(AutosarTree, ClientServer_GridView, ClientServer_NameTextBox);
            componentDefenitionViewController = new ComponentDefenitionController(AutosarTree, ComponentDefenitionName_TextBox, ComponentPorts_GridView, ComponentRunnables_GridView, MultipleInstantiation_CheckBox, AddPerInstanceDefenition_Button, PerInstanceDefenition_Grid, CDataDescription_Grid, AddCDataDescription_Button);
            connectionLineController = new ConnectionLineController(AutosarTree);
            changeViewportScaleController = new ChangeViewportScaleController(scene, ViewPortImage);
            addComponentInstancesController = new AddObjectController(AutosarTree);
            SimpleDataTypeMenu_ApplyButton.Click += SimpleDataTypeMenu_ApplyButton_Click;
            componentPropertiesController = new ComponentPropertiesController(ComponentInstanceName_TextBox, ComponentDefenitionNameTextBox, ComponentInstance_PerInstanceMemory_DataGrid, CDataInstance_DataGrid, AutosarTree);
            compositionInstanceController = new CompositionInstanceController(AutosarTree, CompositionName_TextBox, CompositionPorts_Grid, AddPortToComposition_Button, CompositionTab);
            arrayDataController = new ArrayDataTypeController(AutosarTree, ArrayDataTypeUpdateButton, ArrayDataTypeMenu_NameTextBox, ArrayDataType_SizeTextBox);
            tabHideHelper = new TabHiderHelper(MainTabControl);


            autosarApp.ComponentNameFont = new PortableFontDesc(isbold: true);
            autosarApp.UpdateFontAccordingScale(1);

            openSaveController = new OpenSaveController(autosarApp);

            AutosarTree.Autosar = autosarApp;
            String prevProject = SettingsProvider.GetInstance().LastProjectFileName;
            bool loaded = autosarApp.LoadFromFile(prevProject);
            if (!loaded)
            {
                AutosarApplication.GetInstance().Clear();
                AutosarApplication.GetInstance().CreateNewProject();
                openSaveController.Clear();
            }
            AutosarTree.UpdateAutosarTreeView(null);
            UpdateMainWindowTitle();
            tabHideHelper.ProcessTabs();            
        }

        double lastWidth = 0;
        double lastHeight = 0;

        /* Action by timer */
        public void Render(object sender, EventArgs e)
        {
            scene.RenderScene();  

            /* Render not connected connectionPainter */
            connectionLineController.Render(scene.Context);

            if (lastWidth == 0)
            {
                lastWidth = this.Width;
                lastHeight = this.Height;
            }
            if ((lastWidth != this.Width) || (lastHeight != this.Height))
            {
                lastWidth = this.Width;
                lastHeight = this.Height;

                UpdateImageSize();
            }
        }

        /* Create new component defenition */
        private void AddComponentDefenitionButton_Click(object sender, RoutedEventArgs e)
        {
            string compDefenitionName = "ComponentDefenition";
    
            int index = 0;
            while (autosarApp.ComponentDefenitionsList.FindObject(compDefenitionName) != null)
            {
                index++;
                compDefenitionName = "ComponentDefenition" + index.ToString();
            }

            ComponentDefenition compDefenition = ComponentFabric.GetInstance().CreateComponentDefenition(compDefenitionName);

            autosarApp.ComponentDefenitionsList.Add(compDefenition);

            componentDefenitionViewController.ComponentDefenition = compDefenition;
            AutosarTree.UpdateAutosarTreeView(compDefenition);
            AutosarTree.Focus();
        }

#region MOUSE HANDLE 
        private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                //Scale image with cursor pointer stayed on last plase
                Point currentPoint = e.GetPosition(ViewPortImage);

                //get pointed scene coordinates
                Point sceneCoordinates = scene.MouseToXY(currentPoint);

                autosarApp.UnselectComponents();

                moveObjectsController.Viewport_MouseLeftButtonDown(sceneCoordinates);
                if (moveObjectsController.SelectedObject != null)
                {
                    if (!(moveObjectsController.SelectedObject is CompositionInstance))
                    {
                        AutosarTree.UpdateAutosarTreeView(moveObjectsController.SelectedObject);
                        AutosarTree.Focus();
                    }
                }

                /* Check for creating connection line */
                connectionLineController.Viewport_MouseDown(e, sceneCoordinates);
            }
            else if (e.ClickCount == 2)
            {
                //Scale image with cursor pointer stayed on last plase
                Point currentPoint = e.GetPosition(ViewPortImage);

                //get pointed scene coordinates
                Point sceneCoordinates = scene.MouseToXY(currentPoint);

                autosarApp.UnselectComponents();

                moveObjectsController.Viewport_MouseLeftButtonDown(sceneCoordinates);
                if (moveObjectsController.SelectedObject != null)
                {
                    if (moveObjectsController.SelectedObject is CompositionInstance)
                    {
                       
                        autosarApp.ActiveComposition = moveObjectsController.SelectedObject as CompositionInstance;

                        compositionInstanceController.Composition = autosarApp.ActiveComposition;
                        CompositionProperties_TabItem.IsEnabled = true;
                        SelectElement(autosarApp.ActiveComposition);                        

                        AutosarTree.UpdateAutosarTreeView(moveObjectsController.SelectedObject);
                        AutosarTree.Focus();

                        moveObjectsController.SelectedObject = null;
                    }
                    else if (moveObjectsController.SelectedObject is ComponentInstance)
                    {
                        ComponentInstance inst = (moveObjectsController.SelectedObject as ComponentInstance);
                        componentDefenitionViewController.ComponentDefenition = inst.ComponentDefenition;
                        ComponentDefenitionTab.IsEnabled = true;
                        tabHideHelper.ProcessTabs();
                        tabHideHelper.SelectTab(ComponentDefenitionTab);
                    }
                }
            }
            Render(null, null);
        }

        private void Viewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Scale image with cursor pointer stayed on last plase
            Point currentPoint = e.GetPosition(ViewPortImage);

            //get pointed scene coordinates
            Point sceneCoordinates = scene.MouseToXY(currentPoint);

            /* Add connection line command started */
            connectionLineController.Viewport_MouseLeftButtonUp(sceneCoordinates);

           // moveObjectsController.Viewport_MouseLeftButtonUp();
            Render(null, null);
        }
        
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            Boolean needRedraw = false;

            SceneImageCursorPosition = e.GetPosition(ViewPortImage);
            Point SceneCursorPosition = scene.MouseToXY(SceneImageCursorPosition);

            needRedraw |= changeViewportScaleController.Viewport_MouseMove(e);

            if (connectionLineController.AddConnectionLineActive == false)
            {
                needRedraw |= moveObjectsController.Viewport_MouseMove(SceneCursorPosition, e);
            }

            needRedraw |= connectionLineController.Viewport_MouseMove(e, SceneCursorPosition);
            if (needRedraw != false)
            {
                Render(null, null);
            }
            coordText.Text = SceneCursorPosition.X.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + ", " + SceneCursorPosition.Y.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            coordText.Text += "   |   " + scene.Context.scale.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            /* Change scale */
            changeViewportScaleController.Viewport_MouseWheel(e);
            Render(null, null);
        }

        Point lastLeftPoint = new Point(0, 0);
        Point SceneImageCursorPosition = new Point();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                changeViewportScaleController.Viewport_MouseDown(e);
                if (e.ChangedButton == MouseButton.Left)
                {
                    lastLeftPoint = e.GetPosition(ViewPortImage);
                }
            }
            else if (e.ClickCount == 2)
            {
                if (e.ChangedButton == MouseButton.Middle)
                {
                    changeViewportScaleController.FitWorldToImage(ViewPortImage.ActualWidth, ViewPortImage.ActualHeight);
                }
            }
            Render(null, null);
        }

#endregion

        void UpdateMainWindowTitle()
        {
            Title = "AUTOSAR Gui Editor - " + autosarApp.FileName;
        }
        
        private void AddPortButton_Click(object sender, RoutedEventArgs e)
        {
            bool portHasBeenAdded = componentDefenitionViewController.AddPortButtonClick();
            if (portHasBeenAdded)
            {
                AutosarTree.UpdateAutosarTreeView(null);
            }
        }

        
        private void AddConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            connectionLineController.StartConnection();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            return;
            //if (e.Key == Key.Delete)
            //{
            //    if (moveObjectsController.SelectedObject != null)
            //    {
            //        if (moveObjectsController.SelectedObject is PortConnection)
            //        {
            //            autosarApp.DeleteConnection((PortConnection)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is PortPainter)
            //        {
            //            autosarApp.DeletePort((PortPainter)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is ComponentInstance)
            //        {
            //            autosarApp.Delete((ComponentInstance)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is ComplexDataType)
            //        {
            //            autosarApp.Delete((ComplexDataType)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is SenderReceiverInterface)
            //        {
            //            autosarApp.Delete((SenderReceiverInterface)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is ClientServerInterface)
            //        {
            //            autosarApp.Delete((ClientServerInterface)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is EnumDataType)
            //        {
            //            autosarApp.Delete((EnumDataType)moveObjectsController.SelectedObject);
            //        }
            //        if (moveObjectsController.SelectedObject is ArrayDataType)
            //        {
            //            autosarApp.Delete((ArrayDataType)moveObjectsController.SelectedObject);
            //        }
            //        AutosarTree.UpdateAutosarTreeView(null);                    
            //    }
            //}
        }

        private void AddSimpleDataTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            string SimplaDataTypeTemplateName = "New_SimpleDataType";
            if (autosarApp.SimpleDataTypes.FindObject(SimplaDataTypeTemplateName) != null)
            {
                int index = 0;
                while (autosarApp.SimpleDataTypes.FindObject(SimplaDataTypeTemplateName) != null)
                {
                    index++;
                    SimplaDataTypeTemplateName = "New_SimpleDataType" + index.ToString();
                }
            }

            SimpleDataType datatype = DataTypeFabric.Instance().CreateSimpleDataType(SimplaDataTypeTemplateName);

            autosarApp.SimpleDataTypes.Add(datatype);
            AutosarTree.UpdateAutosarTreeView(datatype);
            AutosarTree.Focus();
        }

        private void AddComplexDataTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            string ComplexDataTypeTemplateName = "New_ComplexDataType";
            if (autosarApp.ComplexDataTypes.FindObject(ComplexDataTypeTemplateName) != null)
            {
                int index = 0;
                while (autosarApp.ComplexDataTypes.FindObject(ComplexDataTypeTemplateName) != null)
                {
                    index++;
                    ComplexDataTypeTemplateName = "New_ComplexDataType" + index.ToString();
                }
            }

            ComplexDataType datatype = DataTypeFabric.Instance().CreateComplexDataType(ComplexDataTypeTemplateName);

            autosarApp.ComplexDataTypes.Add(datatype);
            AutosarTree.UpdateAutosarTreeView(datatype);
            AutosarTree.Focus();
        }

        private void AddArrayDataTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            string ArrayDataTypeTemplateName = "New_ArrayDataType";
            if (autosarApp.ArrayDataTypes.FindObject(ArrayDataTypeTemplateName) != null)
            {
                int index = 0;
                while (autosarApp.ComplexDataTypes.FindObject(ArrayDataTypeTemplateName) != null)
                {
                    index++;
                    ArrayDataTypeTemplateName = "New_ArrayDataType" + index.ToString();
                }
            }

            ArrayDataType datatype = DataTypeFabric.Instance().CreateArrayDataType(ArrayDataTypeTemplateName);

            autosarApp.ArrayDataTypes.Add(datatype);
            AutosarTree.UpdateAutosarTreeView(datatype);
            AutosarTree.Focus();
        }  


        private void EnableTab(TabItem selectedItem)
        {           
            if (selectedItem == SimpleDataTypeTab)
            {
                SimpleDataTypeTab.IsEnabled = true;
            }
            if (selectedItem == ComplexDataTypeTab)
            {
                ComplexDataTypeTab.IsEnabled = true;
            }

            if (selectedItem == SenderReceiverTab)
            {
                SenderReceiverTab.IsEnabled = true;
            }

            if (selectedItem == ClientServerTab)
            {
                ClientServerTab.IsEnabled = true;
            }

            if (selectedItem == Component_Properties_Tab)
            {
                Component_Properties_Tab.IsEnabled = true;
            }

            if (selectedItem == EnumDataTypeTab)
            {
                EnumDataTypeTab.IsEnabled = true;
            }

            if (selectedItem == ArrayDataTypeTab)
            {
                ArrayDataTypeTab.IsEnabled = true;
            }          
        }

        object selectedAutosarTreeObject;

        private void AutosarTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem)
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    tab.IsEnabled = false;                   
                }
                CompositionTab.IsEnabled = true;

                TreeViewItem item = (e.NewValue as TreeViewItem);
                selectedAutosarTreeObject = item.Tag;
                if (item.Tag is SimpleDataType)
                {
                    selectedAutosarTreeObject = item.Tag;
                    simpleDataTypeMenu.DataType = (item.Tag as SimpleDataType);
                    SimpleDataTypeTab.IsEnabled = true;
                    tabHideHelper.SelectTab(SimpleDataTypeTab);
                }
                if (item.Tag is ComplexDataType)
                {
                    complexDataTypeMenu.DataType = (item.Tag as ComplexDataType);
                    ComplexDataTypeTab.IsEnabled = true;
                    tabHideHelper.SelectTab(ComplexDataTypeTab);
                }
                if (item.Tag is EnumDataType)
                {
                    enumsMenu.DataType = (item.Tag as EnumDataType);
                    EnumDataTypeTab.IsEnabled = true;
                    tabHideHelper.SelectTab(EnumDataTypeTab);
                }  
                if  (item.Tag is ComponentDefenition)
                {
                    componentDefenitionViewController.ComponentDefenition = (item.Tag as ComponentDefenition);
                    ComponentDefenitionTab.IsEnabled = true;
                    CompositionTab.IsEnabled = true;
                    tabHideHelper.SelectTab(CompositionTab);
                }

                if (item.Tag is ComponentInstance)
                {
                    componentPropertiesController.Component = (item.Tag as ComponentInstance);
                    SelectElement(item.Tag as ComponentInstance);
                    Component_Properties_Tab.IsEnabled = true;
                }

                if (item.Tag is PortConnection)
                {
                    SelectElement(item.Tag as PortConnection);                 
                }
 
                if (item.Tag is SenderReceiverInterface)
                {
                    senderReceiverInterfaceController.srInterface = (item.Tag as SenderReceiverInterface);
                    SenderReceiverTab.IsEnabled = true;
                    tabHideHelper.SelectTab(SenderReceiverTab);
                }
                if (item.Tag is ClientServerInterface)
                {
                    clientServerInterfaceController.csInterface = (item.Tag as ClientServerInterface);
                    ClientServerTab.IsEnabled = true;
                    tabHideHelper.SelectTab(ClientServerTab);
                }
                if (item.Tag is ArrayDataType)
                {
                    arrayDataController.DataType = (item.Tag as ArrayDataType);
                    ArrayDataTypeTab.IsEnabled = true;
                    tabHideHelper.SelectTab(ArrayDataTypeTab);
                }
                if (item.Tag is CompositionInstance)
                {
                    compositionInstanceController.Composition = (item.Tag as CompositionInstance);
                    CompositionProperties_TabItem.IsEnabled = true;
                    SelectElement(item.Tag as CompositionInstance);
                    tabHideHelper.SelectTab(CompositionTab);
                    Render(null, null);
                    changeViewportScaleController.FitWorldToImage(ViewPortImage.ActualWidth, ViewPortImage.ActualHeight);
                }

                tabHideHelper.ProcessTabs();

                addComponentInstancesController.MouseDownOnTreeViewMenu();
            }

            Render(null, null);
            Render(null, null);
        }

        private void SelectElement(ISelectable element)
        {
            autosarApp.UnselectComponents();
            if (element != null)
            {
                element.Select();
            }
        }

        private void SimpleDataTypeMenu_ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            AutosarTree.UpdateAutosarTreeView(null);
        }


#region COMPLEX DATA TYPE
        /* Chanda datatype for complex datatype field */
        private void ComplexDatatype_DatatypeButton_Click(object sender, RoutedEventArgs e)
        {
            complexDataTypeMenu.ChangeDatatypeButtonClick();
        }

        private void ComplexDataType_GridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;                       
        }

        /* Rename field of complex datatype*/
        private void ComplexDatatype_NameTextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            complexDataTypeMenu.RenameComplexFieldNameTextEdit(sender as TextBox);
        }

        /* Change is pointer property for complex datatype field */
        private void ComplexDataType_IsPointerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            complexDataTypeMenu.IsPointerCheckBoxClick(sender as CheckBox);
        }

        private void ComplexDatatype_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            complexDataTypeMenu.DeleteComplexDatatypeField();
        }

        private void ComplexDataType_NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutosarTree.UpdateAutosarTreeView(null);
        }

        private void ComplexDataType_AddFieldButton_Click(object sender, RoutedEventArgs e)
        {
            complexDataTypeMenu.AddField();
            complexDataTypeMenu.RefreshComplexDatatypeGridView();
        }

#endregion

#region SENDER RECEIVER INTERFACE MENU

        private void AddSenderReceiverMenu_Click(object sender, RoutedEventArgs e)
        {
            SenderReceiverInterface srInterface = senderReceiverInterfaceController.CreateSenderReceiverInterface();

            AutosarTree.UpdateAutosarTreeView(srInterface);
            AutosarTree.Focus();
        }

        private void SenderReceiver_AddField_Button_Click(object sender, RoutedEventArgs e)
        {
            senderReceiverInterfaceController.AddField();
        }

        private void SenderReceiver_IsPointer_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            senderReceiverInterfaceController.IsPointerCheckBoxClick(sender as CheckBox);
        }

        private void SenderReceiver_FieldName_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            senderReceiverInterfaceController.RenameFieldTextEdit(sender as TextBox);
        }

        private void SenderReceiver_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            senderReceiverInterfaceController.ChangeDatatypeButtonClick();
        }

        private void SenderReceiver_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            senderReceiverInterfaceController.DeleteButtonClick();
        }

#endregion

#region CLIENT-SERVER INTERFACE
        private void AddClientServerMenu_Click(object sender, RoutedEventArgs e)
        {
            ClientServerInterface csInterface = clientServerInterfaceController.CreateClientServerInterface();
            AutosarTree.UpdateAutosarTreeView(csInterface);
            AutosarTree.Focus();
        }

        private void ClientServer_FieldName_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            clientServerInterfaceController.RenameFieldTextEdit((sender as TextBox));
        }

        private void ClientServer_DirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clientServerInterfaceController.SelectDirectionClick((sender as ComboBox).SelectedItem.ToString());
        }

        private void ClientServer_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            clientServerInterfaceController.ChangeDatatypeButtonClick();
        }

        private void ClientServer_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            clientServerInterfaceController.DeleteButtonClick();
        }

        private void ClientServer_AddField_Button_Click(object sender, RoutedEventArgs e)
        {
            clientServerInterfaceController.AddField();
        }

        private void ClientServer_AddOperation_Button_Click(object sender, RoutedEventArgs e)
        {
            clientServerInterfaceController.AddOperation();
        }

#endregion

#region COMPONENT PORT
        private void ComponentPorts_PortName_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            componentDefenitionViewController.RenamePortTextEdit((sender as TextBox).Text);
            AutosarTree.UpdateAutosarTreeView(null);
        }


        private void ComponentPorts_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.DeletePortButtonClick();
            AutosarTree.UpdateAutosarTreeView(null);
        }

        private void ComponentPorts_SelectInterface_Button_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.ChangePortInterfaceButtonClick();
        }

        private void ComponentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void AddRunnable_ButtonClick(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.AddRunnableButton_Click();
        }

        private void ComponentGridView_EditFrequency(object sender, TextChangedEventArgs e)
        {
            if (ComponentRunnables_GridView.SelectedIndex >= 0)
            {
                TextBox tb = (TextBox)sender;
                componentDefenitionViewController.UpdateFrequency(tb.Text);
            }
        }

        private void ComponentRunnables_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.DeleteRunnableButtonClick();
        }

        private void ComponentRunnable_FieldName_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            componentDefenitionViewController.RenameRunnable_TextEdit((sender as TextBox).Text);
        }
#endregion


        Point RightMouseButton_DownCoord = new Point(0, 0);

        private void ConnectionPointMenu_DeleteClick(object sender, RoutedEventArgs e)
        {
            if (moveObjectsController.SelectedObject is PortConnection)
            {
                //get pointed scene coordinates
                Point sceneCoordinates = scene.MouseToXY(SceneImageCursorPosition);

                (moveObjectsController.SelectedObject as PortConnection).DeleteAnchor(sceneCoordinates);
            }
        }

        private void AddPointToConnectionLineMenu_Click(object sender, RoutedEventArgs e)
        {
            if (moveObjectsController.SelectedObject is PortConnection)
            {
                (moveObjectsController.SelectedObject as PortConnection).AddPoint(RightMouseButton_DownCoord);
                Render(null, null);
            }
        }

        private void ViewPortImage_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Point currentPoint = new Point(e.CursorLeft, e.CursorTop);
            Point sceneCoordinates = scene.MouseToXY(currentPoint);

            autosarApp.UnselectComponents();

            moveObjectsController.Viewport_MouseLeftButtonDown(sceneCoordinates);

            if (!(moveObjectsController.SelectedObject is PortConnection))
            {
                e.Handled = true;
            }
        }

        private void ViewPort_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Scale image with cursor pointer stayed on last plase
            Point currentPoint = e.GetPosition(ViewPortImage);

            //get pointed scene coordinates
            RightMouseButton_DownCoord = scene.MouseToXY(currentPoint);            
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (autosarApp.FileName != null)
            {
                if (autosarApp.FileName.Length != 0)
                {
                    autosarApp.SaveToFile(autosarApp.FileName);
                }
                else
                {
                    openSaveController.SaveAs();
                }
            }
            else
            {
                openSaveController.SaveAs();
            }
        }

        private void SaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            openSaveController.SaveAs();
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            bool openResult = openSaveController.Open();
            if (openResult)
            {
                UpdateMainWindowTitle();
            }
            changeViewportScaleController.FitWorldToImage(ViewPortImage.ActualWidth, ViewPortImage.ActualHeight);
            AutosarTree.UpdateAutosarTreeView(null);
        }

        public void UpdateImageSize()
        {
            viewPort.Resize((int)ViewPortImage.ActualWidth, (int)ViewPortImage.ActualHeight);
            ViewPortImage.Source = viewPort.WriteableBmp;
            scene.UpdateBitmap(viewPort.WriteableBmp);
        }


        
        private void RunnablesOrderMenu_Click(object sender, RoutedEventArgs e)
        {
            RunnablesOrderForm runnablesOrdersForm = new RunnablesOrderForm();
            runnablesOrdersForm.Owner = this;
            runnablesOrdersForm.ShowForm();
        }

        private void ProjectSettings_Click(object sender, RoutedEventArgs e)
        {
            ProjectSettingsForm projectSettingsForm = new ProjectSettingsForm();
            projectSettingsForm.Owner = this;
            projectSettingsForm.RteGenerationPath = autosarApp.GenerateRtePath;
            projectSettingsForm.ComponentGenerationPath = autosarApp.GenerateComponentsPath;
            projectSettingsForm.Frequency = autosarApp.SystickFrequencyHz;
            projectSettingsForm.ShowDialog();
            if (projectSettingsForm.DialogResult == true)
            {
                autosarApp.SystickFrequencyHz = projectSettingsForm.Frequency;
                autosarApp.GenerateRtePath = projectSettingsForm.RteGenerationPath;
                autosarApp.GenerateComponentsPath = projectSettingsForm.ComponentGenerationPath;
            }
        }

        private void GenerateRTE_Click(object sender, RoutedEventArgs e)
        {
            StringWriter writer = new StringWriter();

            ArxmlTester tester = new ArxmlTester(autosarApp, writer);
            tester.Test();
            String testResult = writer.ToString();
            if(!tester.IsErrorExist(testResult))
            {
                RteGenerator rteGenerator = new RteGenerator();
                bool result = rteGenerator.Generate(); 
                if (result == true)
                {
                    MessageBox.Show("RTE has been generated.");
                }
                else
                {
                    MessageBox.Show("There is a problem with RTE generation.");
                }
            }
            else
            {
                MessageBox.Show("There are errors in the project. RTE generation is impossible. Check project for errors.");
            }
        }

        private void EnumField_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            enumsMenu.DeleteField();
        }

        private void EnumField_NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enumsMenu.RenameFieldName((sender as TextBox));
        }

        private void EnumField_ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enumsMenu.ChangeFieldValue((sender as TextBox));
        }

        private void AddEnumDataTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            string EnumDataTypeTemplateName = "dtEnumDataType";
            if (autosarApp.Enums.FindObject(EnumDataTypeTemplateName) != null)
            {
                int index = 0;
                while (autosarApp.Enums.FindObject(EnumDataTypeTemplateName) != null)
                {
                    index++;
                    EnumDataTypeTemplateName = "dtEnumDataType" + index.ToString();
                }
            }

            EnumDataType datatype = DataTypeFabric.Instance().CreateEnumDataType(EnumDataTypeTemplateName);

            autosarApp.Enums.Add(datatype);
            AutosarTree.UpdateAutosarTreeView(datatype);
            AutosarTree.Focus();
        }

        private void AddEnumField_Button_Click(object sender, RoutedEventArgs e)
        {
            enumsMenu.AddField();
            enumsMenu.RefreshGridView();
        }

        private void SystemErrorsMenu_Click(object sender, RoutedEventArgs e)
        {
            SystemErrorWindow systemErrorWindow = new SystemErrorWindow(autosarApp);
            systemErrorWindow.Owner = this;
            systemErrorWindow.ShowDialog();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            addComponentInstancesController.Clear();
        }

        private void ViewPortImage_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void AutosarTree_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void ViewPortImage_Drop(object sender, DragEventArgs e)
        {
           //Scale image with cursor pointer stayed on last plase
            Point currentPoint = e.GetPosition(ViewPortImage);

            //get pointed scene coordinates
            Point sceneCoordinates = scene.MouseToXY(currentPoint);
            addComponentInstancesController.ViewPortImage_Drop(e, sceneCoordinates.X, sceneCoordinates.Y);
            Render(null, null);
        }

        private void AutosarTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            addComponentInstancesController.MouseDownOnTreeViewMenu();
        }

        private void PerInstanceMemoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            componentDefenitionViewController.PerInstanceMemoryNameTextBox_TextChanged(sender, e);
        }

        private void PerInstanceMemoryName_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.PerInstanceMemoryName_ChangeDatatype_Button_Click(sender, e);
        }

        private void PerInstanceMemoryName_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.PerInstanceMemoryName_DeleteButton_Click(sender, e);
        }

        private void PimDefaultValue_ValueColomn_TextEdit(object sender, TextChangedEventArgs e)
        {
            componentPropertiesController.PimDefaultValue_ValueColomn_TextEdit(sender, e);
        }

        private void CDataDescriptionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            componentDefenitionViewController.CDataNameTextBox_TextChanged(sender, e);
        }

        private void CDataDefenition_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.CData_ChangeDatatype_Button_Click(sender, e);
        }

        private void CDataDescription_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            componentDefenitionViewController.CData_DeleteButton_Click(sender, e);
        }

        private void CDataDefaultValue_ValueColomn_TextEdit(object sender, TextChangedEventArgs e)
        {
            componentPropertiesController.CDataDefaultValue_ValueColomn_TextEdit(sender, e);
        }



        private void AddCompositionButton_Click(object sender, RoutedEventArgs e)
        {
            compositionInstanceController.CreateNewComposition();
        }

        bool AskToDelete(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        bool CouldItemBeDeleted(object tag)
        {
            if (tag.Equals(autosarApp.Compositions.GetMainComposition()))
            {
                return false;
            } else if (tag is ComponentInstance)
            {
                return true;
            }
            else if (tag is SimpleDataType)
            {
                return true;
            }
            else if (tag is EnumDataType)
            {
                return true;
            }
            else if (tag is ComplexDataType)
            {
                return true;
            }
            else if (tag is SenderReceiverInterface)
            {
                return true;
            }
            else if (tag is ClientServerInterface)
            {
                return true;
            }
            else if (tag is CompositionInstance)
            {
                return true;
            }
            else if (tag is ComponentDefenition)
            {
                return true;
            }
            else if (tag is ArrayDataType)
            {
                return true;
            }
            else if (tag is PortConnection)
            {
                return true;
            }
            
            return false;
        }
        private void AutosarTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {              
                if (AutosarTree.SelectedItem is TreeViewItem)
                {
                    TreeViewItem selectedItem = AutosarTree.SelectedItem as TreeViewItem;

                    if (selectedItem.Tag is IGUID)
                    {
                        if (!CouldItemBeDeleted(selectedItem.Tag))
                        {
                            return;
                        }
                        bool delete = AskToDelete("Do you want to delete " + (selectedItem.Tag as IGUID).Name + "?");
                        if (delete == true)
                        {
                            if (selectedItem.Tag is ComponentInstance)
                            {
                                autosarApp.Delete(selectedItem.Tag as ComponentInstance);
                            }
                            else if (selectedItem.Tag is SimpleDataType)
                            {
                                autosarApp.Delete(selectedItem.Tag as SimpleDataType);
                            }
                            else if (selectedItem.Tag is EnumDataType )
                            {
                                autosarApp.Delete(selectedItem.Tag as EnumDataType );
                            }
                            else if (selectedItem.Tag is ComplexDataType )
                            {
                                autosarApp.Delete(selectedItem.Tag as  ComplexDataType);
                            }
                            else if (selectedItem.Tag is SenderReceiverInterface )
                            {
                                autosarApp.Delete(selectedItem.Tag as  SenderReceiverInterface);
                            }
                            else if (selectedItem.Tag is  ClientServerInterface)
                            {
                                autosarApp.Delete(selectedItem.Tag as ClientServerInterface );
                            }
                            else if (selectedItem.Tag is  CompositionInstance)
                            {
                                autosarApp.Delete(selectedItem.Tag as  CompositionInstance);
                                autosarApp.ActiveComposition = autosarApp.Compositions.GetMainComposition();
                            }
                            else if (selectedItem.Tag is  ComponentDefenition)
                            {
                                autosarApp.Delete(selectedItem.Tag as  ComponentDefenition);
                            }
                            else if (selectedItem.Tag is ArrayDataType)
                            {
                                autosarApp.Delete(selectedItem.Tag as ArrayDataType);
                            }
                            else if (selectedItem.Tag is PortConnection)
                            {
                                autosarApp.Delete(selectedItem.Tag as PortConnection);
                            }
                            AutosarTree.UpdateAutosarTreeView(null);
                        }
                    }
                }
            }
        }

        private void CompositionPorts_PortName_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            compositionInstanceController.PortNameTextBox_TextChanged(sender, e);
        }

        private void CompositionPort_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            compositionInstanceController.Port_DeleteButton_Click(sender, e);
        }

        private void CompositionPorts_SelectInterface_Button_Click(object sender, RoutedEventArgs e)
        {
            compositionInstanceController.ChangePortInterfaceButtonClick();
        }

        private void CheckErrorsMenu_Click(object sender, RoutedEventArgs e)
        {
            StringWriter writer = new StringWriter();

            ArxmlTester tester = new ArxmlTester(autosarApp, writer);
            tester.Test();
            ConsoleWorker.GetInstance().Clear();
            ConsoleWorker.GetInstance().AddText(writer.ToString());
            ConsoleWorker.GetInstance().Show();            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /* Get main composition and open it */
            CompositionInstance mainComposition = AutosarApplication.GetInstance().Compositions.GetMainComposition();

            compositionInstanceController.Composition = mainComposition;
            CompositionProperties_TabItem.IsEnabled = true;
            SelectElement(mainComposition);
            tabHideHelper.SelectTab(CompositionTab);
            tabHideHelper.ProcessTabs();
            Render(null, null);
            changeViewportScaleController.FitWorldToImage(ViewPortImage.ActualWidth, ViewPortImage.ActualHeight);
            AutosarApplication.GetInstance().UpdateFontAccordingScale(scene.Context.Scale);

            Render(null, null);
            //loaded = true;
        }

        Boolean loaded = false;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (loaded)
            {
                UpdateImageSize();
                // MouseWheelEventArgs fakeMouseWheel = new MouseWheelEventArgs(null, 0, 0);
                //changeViewportScaleController.Viewport_MouseWheel(fakeMouseWheel);
                //changeViewportScaleController.FitWorldToImage(ViewPortImage.ActualWidth, ViewPortImage.ActualHeight);
                Render(null, null);
                Render(null, null);
            }
        }

        private void ViewPortImage_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }

        String lastTestEnvironmentDir;

        private void GenerateTestEnvironment_Click(object sender, RoutedEventArgs e)
        {
            ComponentDefenition compDef = null;
            if (selectedAutosarTreeObject is ComponentDefenition)
            {
                compDef = (ComponentDefenition)selectedAutosarTreeObject;
            }
            else if (selectedAutosarTreeObject is ComponentInstance)
            {
                compDef = ((ComponentInstance)selectedAutosarTreeObject).ComponentDefenition;
            }

            if (compDef != null)
            {
                
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (lastTestEnvironmentDir != null)
                {
                    dialog.SelectedPath = lastTestEnvironmentDir;
                }
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    lastTestEnvironmentDir = dialog.SelectedPath;
                    TestRteEnvironmentGenerator generator = new TestRteEnvironmentGenerator();
                    generator.GenerateRteEnvironment(compDef, dialog.SelectedPath);
                    MessageBox.Show("Test environment has been generated for " + compDef.Name);
                }
            }
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            AutosarApplication.GetInstance().Clear();
            AutosarApplication.GetInstance().CreateNewProject();
            openSaveController.Clear();
            AutosarTree.UpdateAutosarTreeView();
        }        
    }

}
