using System;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ProjetWin
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("LogoOpale.jpg", UriKind.Relative);
            bi3.EndInit();
            myImage3.Stretch = Stretch.Fill;
            myImage3.Source = bi3;
            
        }
        /*
        Picture image = null;
        void select_display_image(Image dest_i, System.Windows.Controls.Label dest_l)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image documents (.bmp)|*.bmp"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string chemin = dlg.FileName;
                dest_i.Source = new BitmapImage(new Uri(@chemin));
                // dest_l.Content = Path.GetFileNameWithoutExtension(chemin);
                dest_l.Content = chemin;
                byte[] file = File.ReadAllBytes(chemin);
                image = new Picture(file);
            }
        }
        */
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChooseFileInBrowser(object sender, RoutedEventArgs e) // permet de selectionner un fichier directement depuis l'explorateur de fichier
        {
            OpenFileDialog opentest = new OpenFileDialog();
            opentest.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            opentest.Multiselect = false;
            opentest.ShowDialog();
            filename.FontSize = 16;
            filename.Text = opentest.FileName;
        }

        private void LaunchIdentificationFile(object sender, RoutedEventArgs e) // associé au bouton "upload the selected file", lance la fonction qui va lire le document et qui va lui donne un type
        {
            InformationSupp.Visibility = Visibility.Visible;

            string result = "fiche de paie"; //par exemple
            ResultatIdentification.FontSize = 16;
            ResultatIdentification.Content = "The file is identified as " + result + ".";

            if (result == "fiche de paie") //avant cette boucle Propriete1 et Propriete2 sont invisible
            {
                Propriete1.Content = "Employeur";
                Propriete1.Visibility = Visibility.Visible;
                String Employeur = ResultProp1.Text;
            }
            if(result =="Feuille de soin")
            {
                Propriete1.Content = "Délivrée par : ";
                Propriete2.Content = "Soins concernés";
                Propriete1.Visibility = Visibility.Visible;
                Propriete2.Visibility = Visibility.Visible;
                String Delivreur = ResultProp1.Text;
                String SoinsType = ResultProp1.Text;
            }
            if(result == "Feuille d'impôts")
            {
                Propriete1.Content = "Année de facturation :";
                Propriete1.Visibility = Visibility.Visible;
                String Annee = ResultProp1.Text;
            }
        }

        private void Validation(object sender, RoutedEventArgs e) // fonction du bouton qui valide la validationdes infos associées au document
        {
            Propriete1.Content = "Label 1";
            Propriete2.Content = "Label 2";
            String metadata1 = ResultProp1.Text;
            String metadata2 = ResultProp2.Text;
            ResultProp1.Text = " ";
            ResultProp2.Text = " ";
            InformationSupp.Visibility = Visibility.Hidden;
        }
 //--------------------------------------------------------------------------------------------------------------------------------------------------------Onglet filtre
        private void AddFilter(object sender, RoutedEventArgs e)
        {

        }

        private void PaieOption(object sender, RoutedEventArgs e)
        {
            if (PaieFilter.IsChecked == true)
            {
                LabelEmployeur.Visibility = Visibility.Visible;
                Employeur.Visibility = Visibility.Visible;
            }
            if (PaieFilter.IsChecked == false)
            {
                LabelEmployeur.Visibility = Visibility.Collapsed;
                Employeur.Visibility = Visibility.Collapsed;
            }
        }

        private void ImpotOption(object sender, RoutedEventArgs e)
        {
            if(ImpotFilter.IsChecked==true)
            {
                LabelAnnee.Visibility = Visibility.Visible;
                Année.Visibility = Visibility.Visible;
            }
            if (ImpotFilter.IsChecked == false)
            {
                LabelAnnee.Visibility = Visibility.Collapsed;
                Année.Visibility = Visibility.Collapsed;
            }
        }

        private void SoinOption(object sender, RoutedEventArgs e)
        {
            if (SoinFilter.IsChecked==true)
            {
                LabelTypSoin.Visibility = Visibility.Visible;
                TypeSoin.Visibility = Visibility.Visible;
                LabelDelivreur.Visibility = Visibility.Visible;
                DélivreurSoin.Visibility = Visibility.Visible;   
            }
            if (SoinFilter.IsChecked == false)
            {
                LabelTypSoin.Visibility = Visibility.Collapsed;
                TypeSoin.Visibility = Visibility.Collapsed;
                TypeSoin.Text = " ";
                LabelDelivreur.Visibility = Visibility.Collapsed;
                DélivreurSoin.Visibility = Visibility.Collapsed;
                DélivreurSoin.Text = " ";
            }
        }
    }
}
