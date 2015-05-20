using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ServerLogica.Models;

namespace ServerLogica
{
    public partial class FrmMain : Form
    {
        private int numberOfSyncs;
        private string sendTo, subject, emailText;
        private Email email; 
        private ProjectBurocenterDataContext projectBurocenterDataContext;
        private bool run;
        private List<Verbruik_Ligplaat> alreadyKnownProduct;
        public FrmMain()
        {
            InitializeComponent();
            projectBurocenterDataContext = new ProjectBurocenterDataContext();
            numberOfSyncs = SyncCount();
            sendTo = txtSend.Text;
            subject = "Synchronization";
            emailText = "There was a synchronization at " + DateTime.Now.ToLongDateString() + " "
                + "\nKind regards \nProject Burocenter";
            email = new Email(sendTo, subject, emailText);
            alreadyKnownProduct = new List<Verbruik_Ligplaat>();
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            run = true;
            Task.Factory.StartNew(NewTask);
            ChangeVisibility();
            

        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            run = false;
            ChangeVisibility(); 
        }

        private int SyncCount()
        {
            var query = from a in projectBurocenterDataContext.logs select a;
            return query.Count();
        } 

        private void NewTask()
        {
            while (run)
            {
                if (SyncCount() > numberOfSyncs && (CheckMinimumStock() || NeedToSendPdf()))
                {
                    numberOfSyncs = SyncCount();
                    PDFCreate pdf = new PDFCreate();
                    pdf.CreateDummyDocument();
                    email.SendPDF(pdf.FilePathAndName);
                }
                System.Threading.Thread.Sleep(60000);
            }
            
        }

        private bool NeedToSendPdf()
        {
            bool needToSend = false;

            var queryOpdrachten = from a in projectBurocenterDataContext.Opdrachts select a;
            var queryOpdrachtWnemers = from a in projectBurocenterDataContext.Opdracht_Werknemers select a;
            var queryWnemers = from a in projectBurocenterDataContext.Werknemers select a;
            var queryPrestaties = from a in projectBurocenterDataContext.Prestaties select a;
            var queryPrSoort = from a in projectBurocenterDataContext.Prestatiesoorts select a;
            var queryArtikels = from a in projectBurocenterDataContext.Artikels select a;
            var queryVerbruikArt = from a in projectBurocenterDataContext.Verbruikartikels select a;

            List<Opdracht> opdrachten = queryOpdrachten.ToList();

            foreach(Opdracht op in opdrachten)
            {
                if (op.afgewerkt)
                {
                    needToSend = true;
                    List<Opdracht_Werknemer> opw = queryOpdrachtWnemers.Where(t => t.opdrachtid == op.opdrachtid).ToList();
                    // Meerdere werknemers implementeren!
                    Werknemer wnemer = queryWnemers.Where(t => t.medewerkerid == opw[0].werknemerid).ToList()[0];
                    Prestatie prestatie = queryPrestaties.Where(t => t.opdrachtid == op.opdrachtid).ToList()[0];
                    Prestatiesoort psoort = queryPrSoort.Where(t => t.prestatieid == prestatie.prestatiesoortid).ToList()[0];
                    List<Artikel> artikels = queryArtikels.ToList();
                    List<Verbruikartikel> vbrArt = queryVerbruikArt.Where(t => t.opdrachtid == op.opdrachtid).ToList();
                    PDFCreate pdf = new PDFCreate(op.opdrachtid.ToString() + " " + op.omschrijving);
                    //pdf.CreateDocument(op, opw, wnemer, prestatie, psoort, artikels, vbrArt);
                    pdf.CreateDummyDocument();
                    email.SendPDF(pdf.FilePathAndName);
                }
            }

            return needToSend;

        }

        private void ChangeVisibility()
        {
            cmdStop.Visible = !cmdStop.Visible;
            cmdStart.Visible = !cmdStart.Visible;
            lblSend.Visible = !lblSend.Visible;
            txtSend.Visible = !txtSend.Visible;
        }

        private bool CheckMinimumStock()
        {
            bool underMinimum = false;
            List<Verbruik_Ligplaat> verbruikLigplaatsen = new List<Verbruik_Ligplaat>();
            var queryVerbruikLigplaats = from a in projectBurocenterDataContext.Verbruik_Ligplaats select a;
            verbruikLigplaatsen = queryVerbruikLigplaats.ToList();
            var queryLigplaats = from a in projectBurocenterDataContext.Ligplaats select a;
            List<Ligplaat> ligplaatsen = queryLigplaats.ToList();
            var queryArtikel = from a in projectBurocenterDataContext.Artikels select a;
            List<Artikel> artikels = queryArtikel.ToList();
            string tempMessage = "Er is een artikel die onder zijn minimum stock zit:\n";
            foreach (Verbruik_Ligplaat vl in verbruikLigplaatsen)
                if (vl.aantalStock < vl.minStock && !isAlreadyKnown(vl))
                {
                    underMinimum = true;
                    tempMessage += artikels[vl.artikelid - 1].omschrijving + " " + artikels[vl.artikelid - 1].barcode + "\nOp ligplaats:"
                        + ligplaatsen[vl.ligplaatsid - 1].omschrijving + "\n";
                    if (ChangeSender(vl) != "")
                        email.SendTo = ChangeSender(vl);
                    email.EmailText = tempMessage;
                    email.SendEmail();
                }                
            return underMinimum; 
        }

        private bool isAlreadyKnown(Verbruik_Ligplaat vl)
        {
            bool alreadyKnown = false;
            foreach (Verbruik_Ligplaat vlp in alreadyKnownProduct)
                if (vlp.ligplaatsid == vl.ligplaatsid && vlp.artikelid == vl.artikelid)
                {
                    alreadyKnownProduct.Add(vl);
                    alreadyKnown = true;
                }
            return alreadyKnown;
        }

        private string ChangeSender(Verbruik_Ligplaat vl)
        {
            var queryLigplaats = from a in projectBurocenterDataContext.Ligplaats select a;
            List<Ligplaat> ligplaatsen = queryLigplaats.ToList();
            var queryHoofdligplaats = from a in projectBurocenterDataContext.Hoofdligplaats select a;
            List<Hoofdligplaat> hoofdligplaatsen = queryHoofdligplaats.ToList();
            var queryWerknemer = from a in projectBurocenterDataContext.Werknemers select a;
            List<Werknemer> werknemers = queryWerknemer.ToList();
            Hoofdligplaat hoofdligplaats = null;
            foreach (Hoofdligplaat hlp in hoofdligplaatsen)
                if (hlp.ligplaats == vl.ligplaatsid)
                    hoofdligplaats = hlp;
            if (hoofdligplaats != null)
            {
                int verantwoordelijkeid = (int)ligplaatsen[hoofdligplaats.hoofdligplaats - 1].verantwoordelijkeid;
                return werknemers[verantwoordelijkeid - 1].emailadres;
            }
            else
                return "";

        }



    }
}
