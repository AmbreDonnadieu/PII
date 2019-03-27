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

        private void LaunchIdentificationFile(object sender, RoutedEventArgs e) // associé au bouton "upload the selected file", lance la fonction qui va lire le document et qui va lui donne un type
        {
            InformationSupp.Visibility = Visibility.Visible;
            string path = filename.Text;
            path = path.Replace("\\", "/");
            MakeOCRRequest(path).Wait();
            Document doc = CreateDocument(GetTextfromJSON(File.ReadAllText("JSON.txt")), database);         

            if (doc is Fiche_de_Paie) //avant cette boucle Propriete1 et Propriete2 sont invisible
            {
                Propriete1.Content = "Employeur";
                Propriete1.Visibility = Visibility.Visible;
                (doc as Fiche_de_Paie).Employeur = ResultProp1.Text;
            }
            else if(doc is Feuille_de_Soins)
            {
                Propriete1.Content = "Délivrée par : ";
                Propriete2.Content = "Soins concernés";
                Propriete1.Visibility = Visibility.Visible;
                Propriete2.Visibility = Visibility.Visible;
                (doc as Feuille_de_Soins).DelivrePar = ResultProp1.Text;
                (doc as Feuille_de_Soins).SoinsConcernes = ResultProp2.Text;
            }
            database.Add(doc);
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

        private void PaieOption(object sender, RoutedEventArgs e) { }
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
            for (int i = 0; i < JSONupper.Count; i++)
            {
                for (int j = 0; j < upper.Count; j++)
                {
                    if (JSONupper[i] == upper[j]) count++;
                }
            }
            float p = count/upper.Count;
            if (p >= 0.75) return p;
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
                for (int j = 0; i < database[i].Id.Length / 2; j++)
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
    }
}
