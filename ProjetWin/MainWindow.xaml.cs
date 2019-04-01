using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        //Clef Azure pour les services cognitif de Computer Vision 
        const string subscriptionKey = "1e955ee8d59f414c82402e622c673a88";
        const string uriBase = "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/ocr";

        //database
        List<Document> database = new List<Document>();

        private void ChooseFileInBrowser(object sender, RoutedEventArgs e) // permet de selectionner un fichier directement depuis l'explorateur de fichier
        {
            OpenFileDialog opentest = new OpenFileDialog();
            opentest.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            opentest.Multiselect = false;
            opentest.ShowDialog();
            filename.FontSize = 16;
            filename.Text = opentest.FileName;
        }

        private async void LaunchIdentificationFile(object sender, RoutedEventArgs e) // associé au bouton "upload the selected file", lance la fonction qui va lire le document et qui va lui donne un type
        {
            string path = filename.Text;
            string docname =
            path = path.Replace("\\", "/");
            await Task.Run(() =>
            {
                MakeOCRRequest(path).Wait();
            });
            Document doc = CreateDocument(GetTextfromJSON(File.ReadAllText("JSON.txt")), database);
            if (doc == null)
            {
                MessageBox.Show("Error, file not recongnized.");
                return;
            }
            else ResultatIdentification.Content = doc.GetType().ToString();
            doc.Id = GenerateId(NumberForFileType(doc), database);
            ValiderMetaData.Visibility = Visibility.Visible;
            TheDate.Visibility = Visibility.Visible;
            TheDatePicker.Visibility = Visibility.Visible;

            if (doc is Fiche_de_Paie) //avant cette boucle Propriete1 et Propriete2 sont invisible
            {
                Propriete1.Content = "Employeur";
                ResultProp1.Visibility = Visibility.Visible;
                Propriete1.Visibility = Visibility.Visible;
                File.Move(path, "C:/Users/tfabr/Desktop/Dossier Test PI2/Fiche de Paie" + GetDocName(path));
                doc.Path = "C:/Users/tfabr/Desktop/Dossier Test PI2/Fiche de Paie" + GetDocName(path);
            }
            else if (doc is Feuille_de_Soins)
            {
                Propriete1.Content = "Délivrée par : ";
                Propriete2.Content = "Soins concernés";
                Propriete1.Visibility = Visibility.Visible;
                Propriete2.Visibility = Visibility.Visible;
                ResultProp1.Visibility = Visibility.Visible;
                ResultProp2.Visibility = Visibility.Visible;
                File.Move(path, "C:/Users/tfabr/Desktop/Dossier Test PI2/Feuille de Soins" + GetDocName(path));
                doc.Path = "C:/Users/tfabr/Desktop/Dossier Test PI2/Feuille de Soins" + GetDocName(path);
            }
            else if (doc is Declaration_Impots)
            {
                File.Move(path, "C:/Users/tfabr/Desktop/Dossier Test PI2/Déclaration d'Impots" + GetDocName(path));
                doc.Path = "C:/Users/tfabr/Desktop/Dossier Test PI2/Déclaration d'Impots" + GetDocName(path);
            }
            database.Add(doc);
        }

        private void Validation(object sender, RoutedEventArgs e) // fonction du bouton qui valide la validationdes infos associées au document
        {
            database[database.Count - 1].DateDuDocument = TheDatePicker.DisplayDate;

            if (database[database.Count - 1] is Fiche_de_Paie) (database[database.Count - 1] as Fiche_de_Paie).Employeur = ResultProp1.Text;
            else if (database[database.Count - 1] is Feuille_de_Soins)
            {
                (database[database.Count - 1] as Feuille_de_Soins).DelivrePar = ResultProp1.Text;
                (database[database.Count - 1] as Feuille_de_Soins).SoinsConcernes = ResultProp2.Text;
            }

            Propriete1.Visibility = Visibility.Hidden;
            Propriete2.Visibility = Visibility.Hidden;
            ResultProp1.Visibility = Visibility.Hidden;
            ResultProp2.Visibility = Visibility.Hidden;
            ValiderMetaData.Visibility = Visibility.Hidden;
            TheDate.Visibility = Visibility.Hidden;
            TheDatePicker.Visibility = Visibility.Hidden;
            ResultatIdentification.Content = "";
        }

        private void ListBoxDoc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = "";
            string id = RecupID(ListBoxDoc.SelectedItem.ToString());
            foreach (Document paper in database)
            {
                if (paper.Id == id)
                {
                    path = paper.Path;
                }
            }
            if (path != "")
            {
                System.Diagnostics.Process.Start(path);
            }

        }

        private string RecupID(string phrase)
        {
            int debut = 0;
            int longueur = 8;
            for (int i = 0; i < phrase.Length; i++)
            {
                if (phrase[i] == ':')
                {
                    debut = i + 2;
                }
            }
            return phrase.Substring(debut, longueur);
        }

        private void PaieOption(object sender, RoutedEventArgs e)
        {
            if (PaieFilter.IsChecked == true)
            {
                LabelAnnee.Visibility = Visibility.Visible;
                Année.Visibility = Visibility.Visible;
                Search.Visibility = Visibility.Visible;
                LabelEmployeur.Visibility = Visibility.Visible;
                Employeur.Visibility = Visibility.Visible;
                foreach (Document paper in database)
                {
                    if (paper is Fiche_de_Paie)
                    {
                        ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                    }
                }
            }
            else
            {
                LabelAnnee.Visibility = Visibility.Collapsed;
                Année.Visibility = Visibility.Collapsed;
                if (SoinFilter.IsChecked == false) Search.Visibility = Visibility.Collapsed;
                LabelEmployeur.Visibility = Visibility.Collapsed;
                Employeur.Visibility = Visibility.Collapsed;
                for (int i = ListBoxDoc.Items.Count - 1; i >= 0; i--)
                {
                    if (ListBoxDoc.Items.GetItemAt(i).ToString().Substring(0, 13) == "Fiche_de_Paie")
                    {
                        ListBoxDoc.Items.RemoveAt(i);
                    }
                }
            }

        }
        private void ImpotOption(object sender, RoutedEventArgs e)
        {
            if (ImpotFilter.IsChecked == true)
            {
                LabelAnnee.Visibility = Visibility.Visible;
                Année.Visibility = Visibility.Visible;
                Search.Visibility = Visibility.Visible;
                foreach (Document paper in database)
                {
                    if (paper is Declaration_Impots)
                    {
                        ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                    }
                }
            }
            else
            {
                LabelAnnee.Visibility = Visibility.Collapsed;
                Année.Visibility = Visibility.Collapsed;
                Search.Visibility = Visibility.Collapsed;
                for (int i = ListBoxDoc.Items.Count - 1; i >= 0; i--)
                {
                    if (ListBoxDoc.Items.GetItemAt(i).ToString().Substring(0, 18) == "Declaration_Impots")
                    {
                        ListBoxDoc.Items.RemoveAt(i);
                    }
                }
            }
        }
        private void SoinOption(object sender, RoutedEventArgs e)
        {
            if (SoinFilter.IsChecked == true)
            {
                LabelAnnee.Visibility = Visibility.Visible;
                Année.Visibility = Visibility.Visible;
                Search.Visibility = Visibility.Visible;
                LabelDelivreur.Visibility = Visibility.Visible;
                DélivreurSoin.Visibility = Visibility.Visible;
                foreach (Document paper in database)
                {
                    if (paper is Feuille_de_Soins)
                    {
                        ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                    }
                }
            }
            else
            {
                LabelAnnee.Visibility = Visibility.Collapsed;
                Année.Visibility = Visibility.Collapsed;
                Search.Visibility = Visibility.Collapsed;
                LabelDelivreur.Visibility = Visibility.Collapsed;
                DélivreurSoin.Visibility = Visibility.Collapsed;
                for (int i = ListBoxDoc.Items.Count - 1; i >= 0; i--)
                {
                    if (ListBoxDoc.Items.GetItemAt(i).ToString().Substring(0, 16) == "Feuille_de_Soins")
                    {
                        ListBoxDoc.Items.RemoveAt(i);
                    }
                }
            }
        }

        private void SearchingWithFilters(object sender, RoutedEventArgs e)
        {
            for (int i = ListBoxDoc.Items.Count - 1; i >= 0; i--)
            {
                ListBoxDoc.Items.RemoveAt(i);
            }
            //Fiche de Paie
            if(PaieFilter.IsChecked == true)
            {
                foreach (Document paper in database)
                {
                    if(Année.Text == "")
                    {
                        if(Employeur.Text == "")
                        {
                            if(paper is Fiche_de_Paie)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                        }
                        else if(paper is Fiche_de_Paie) if((paper as Fiche_de_Paie).Employeur == Employeur.Text)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                    }
                    else if(Employeur.Text == "")
                    {
                        if(paper is Fiche_de_Paie && Convert.ToString(paper.DateDuDocument.Year) == Année.Text)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                        else if(paper is Fiche_de_Paie ) if((paper as Fiche_de_Paie).Employeur == Employeur.Text && Convert.ToString(paper.DateDuDocument.Year) == Année.Text)
                                {
                                    ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                                }
                    }
                }
            }
            //Feuille de Soins
            if (SoinFilter.IsChecked == true)
            {
                foreach (Document paper in database)
                {
                    if (Année.Text == "")
                    {
                        if (DélivreurSoin.Text == "")
                        {
                            if (paper is Feuille_de_Soins)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                        }
                        else if (paper is Feuille_de_Soins) if ((paper as Feuille_de_Soins).DelivrePar == DélivreurSoin.Text)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                    }
                    else if (Employeur.Text == "")
                    {
                        if (paper is Feuille_de_Soins && Convert.ToString(paper.DateDuDocument.Year) == Année.Text)
                            {
                                ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                            }
                            else if (paper is Feuille_de_Soins) if ((paper as Feuille_de_Soins).DelivrePar == DélivreurSoin.Text && Convert.ToString(paper.DateDuDocument.Year) == Année.Text)
                                {
                                    ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                                }
                    }
                }
            }
            //Déclaration d'impots
            if(ImpotFilter.IsChecked == true)
            {
                foreach(Document paper in database)
                {
                    if (Année.Text == "")
                    {
                        if(paper is Declaration_Impots)
                        {
                            ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                        }
                    }
                    else if (paper is Declaration_Impots && Convert.ToString(paper.DateDuDocument.Year) == Année.Text)
                    {
                        ListBoxDoc.Items.Add(paper.GetType().ToString().Substring(10) + " ID : " + paper.Id + " Date : " + paper.DateDuDocument);
                    }
                }
            }
        }

        private void AddFilter(object sender, RoutedEventArgs e) { }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------Onglet filtre

        //Fonction d'utilisation de l'API 
        //param:
        //@imageFilePath : le chemin du fichier à lire
        static async Task MakeOCRRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters = "language=unk&detectOrientation=true";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Lecture du contenu et stockage sous forme d'un tableau d'octets
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                //Ecriture du JSON dans un fichier texte
                //Qui sera lu par la suite
                string file = "JSON.txt";
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine(JToken.Parse(contentString).ToString());
                sw.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        //Fonction de récupération du contenu d'un fichier 
        //sous forme d'un tableau d'octets
        //param:
        //@imageFilePath : le chemin du fichier à lire
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        //Fonction de récupération du texte depuis 
        //le fichier JSON
        //param : 
        //@message : le fichier JSON sous forme d'une seule chaine de caractères
        static List<string> GetTextfromJSON(string message)
        {

            List<string> text = new List<string>();
            JObject o = JObject.Parse(message);
            IEnumerable<JToken> list = o.SelectTokens("$...text");
            foreach (JToken item in list)
            {
                text.Add(item.ToString());
            }
            return text;
        }

        //Fonction de conversion d'une chaine de caractères 
        //sous forme de liste de string
        //param:
        //@text : la chaine à convertir
        static List<string> ConvertToListString(string text)
        {
            List<string> result = new List<string>();

            string word = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ';')
                {
                    result.Add(word);
                    word = "";
                }
                else word += text[i];
            }
            return result;
        }

        //Fonction de calcul de la probabilité
        //Pour qu'un document corresponde au type du champ lexical donné en paramètre
        //param:
        //@JSON : le texte du JSON sous forme de liste de string
        //@lexique : la liste des mots du champ lexical 
        static float Lexique(List<string> JSON, string lexique)
        {
            string l = File.ReadAllText(lexique);
            List<string> keys = ConvertToListString(l);
            List<string> upper = new List<string>();
            foreach (string s in keys)
            {
                string S = s.ToUpper();
                upper.Add(S);
            }
            List<string> JSONupper = new List<string>();
            foreach (string s in JSON)
            {
                string S = s.ToUpper();
                JSONupper.Add(S);
            }
            float count = 0;
            for (int i = 0; i < upper.Count; i++)
            {
                for (int j = 0; j < JSONupper.Count; j++)
                {
                    if (upper[i] == JSONupper[j])
                    {
                        count++;
                        break;
                    }
                }
            }
            float p = count / upper.Count;
            if (p >= 0.33) return p;
            else return 0;
        }

        //Crée une instance de document suivant la catégorie déterminée par la fonction Lexique
        static Document CreateDocument(List<string> text, List<Document> database)
        {
            List<string> infos = new List<string>();
            if (Lexique(text, "lexique fiche de paie.txt") > 0)
            {
                Fiche_de_Paie doc = new Fiche_de_Paie();
                return doc;
            }
            else if (Lexique(text, "lexique feuille de soins.txt") > 0)
            {
                Feuille_de_Soins doc = new Feuille_de_Soins();
                return doc;
            }
            else if (Lexique(text, "lexique déclaration impots.txt") > 0)
            {
                Declaration_Impots doc = new Declaration_Impots();
                return doc;
            }
            return null;
        }
        // ---------------------------------------------------------------------------------------------New folder page
        private void SelectFolder(object sender, RoutedEventArgs e)
        {

            // before this go to Outils->Gestionnaire de packages NuGet->Console de package NuGet 
            //and put this "Install-Package Ookii.Dialogs" to install the package
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                YourFolder.FontSize = 16;
                YourFolder.Content = dialog.SelectedPath;
            }
            //Your_folder est le dossier sélectionné par l'utilisateur 
            // en gros faut voir dans les lexiques si le nom du dossier est dedans 
            // si non il faut demander de créer un nouveau dossier 
            // si oui on donne les fichiers manquants pour compléter le dossier

        }

        private void SelectRequest(object sender, RoutedEventArgs e)
        {
            string[] theRequest = File.ReadAllLines(Request.Text + ".txt");
            if (theRequest != null)
            {
                int number = 0;
                Document doc = new Document();
                for (int i = 0; i < theRequest.Length; i++)
                {
                    if (i % 2 == 0) number = Convert.ToInt32(theRequest[i]);
                    else
                    {
                        while (number > 0)
                        {
                            foreach (Document paper in database)
                            {
                                if (Convert.ToString(paper.GetType()) == theRequest[i] && doc.DateDuDocument < paper.DateDuDocument)
                                {
                                    doc = paper;
                                }
                            }
                            File.Move(doc.Path, Convert.ToString(YourFolder.Content) + GetDocName(doc.Path));
                            number--;
                        }
                    }
                }
                MessageBox.Show("Operation completed.");
            }
            else MessageBox.Show("Error, invalid request.");
        }

        //Génère un id pour le document
        //Les 4 premiers chiffes désignent le type de document
        //Les 4 suivants sont une désignation purement numérique
        static string GenerateId(int fileType, List<Document> database)
        {
            string id = "";
            if (fileType < 10) id = "000" + fileType;
            else if (fileType < 100) id = "00" + fileType;
            else if (fileType < 1000) id = "0" + fileType;
            else id += fileType;

            string idFileType = ""; //Les 4 premiers chiffes de l'id
            int count = 0;

            for (int i = 0; i < database.Count; i++)
            {
                for (int j = 0; j < database[i].Id.Length / 2; j++)
                {
                    idFileType += database[i].Id[j];
                }
                if (idFileType == id) count++;
            }

            if (count == 0) id += "0000";
            else if (count < 10) id += "000" + count;
            else if (count < 100) id += "00" + count;
            else if (count < 1000) id += "0" + count;
            else id += count;

            return id;
        }

        //Attribue un entier pour chaque type de document
        static int NumberForFileType(Document doc)
        {
            if (doc is Fiche_de_Paie) return 0;
            else if (doc is Feuille_de_Soins) return 1;
            else if (doc is Declaration_Impots) return 2;
            else return -1;
        }

        //retourne le nom d'un document à partir du chemin d'accès
        static string GetDocName(string path)
        {
            int last = -1;
            for (int i = 0; i < path.Length; i++) if (path[i] == '/') if (path[i] == '/') last = i;
            string result = "";
            for (int i = last; i < path.Length; i++) result += path[i];
            return result;
        }

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

            database.Add(new Feuille_de_Soins("00010001", new DateTime(2019, 2, 14), "Mr. Medecin", "", "Entorce"));
            database.Add(new Feuille_de_Soins("00010002", new DateTime(2018, 11, 5), "Hopital de Paris", ""));
            database.Add(new Fiche_de_Paie("00000001", new DateTime(2019, 2, 1), "Esilv", ""));
            database.Add(new Fiche_de_Paie("00000002", new DateTime(2019, 1, 1), "Esilv", ""));
            database.Add(new Fiche_de_Paie("00000003", new DateTime(2018, 12, 1), "Esilv", ""));
            database.Add(new Declaration_Impots("00020001", new DateTime(2018, 12, 12), ""));
            database.Add(new Declaration_Impots("00020002", new DateTime(2017, 12, 12), ""));
        }
    }
}