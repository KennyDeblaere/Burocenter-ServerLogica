using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iTextSharp.text;
using iTextSharp;
using System.IO;
using iTextSharp.text.pdf;

namespace ServerLogica.Models
{
    class PDFCreate
    {
        private string filePathAndName;
        private Document document;

        public string FilePathAndName
        {
            get { return filePathAndName; }
            private set { filePathAndName = value; }
        }

        public Document Document
        {
            get { return document; }
        }

        public PDFCreate(string documentname = "Pdf")
        {
            FilePathAndName = "C:\\Users\\Administrator\\Documents\\PDFS\\" + documentname + ".pdf";
            
        }

        public void CreateDocument(Opdracht opdracht, List<Opdracht_Werknemer> opw, Werknemer werknemer, Prestatie prestatie, Prestatiesoort prestatiesoort, List<Artikel> artikels, List<Verbruikartikel> verbruikartikel)
        {
            document = new Document();
            PdfWriter wri = PdfWriter.GetInstance(document, new FileStream(FilePathAndName, FileMode.Create));
            document.Open();
            document.Add(CreateParagraph(opdracht, opw, werknemer, prestatie, prestatiesoort, artikels, verbruikartikel));
            document.Close();
        }

        public void CreateDummyDocument()
        {
            document = new Document();
            PdfWriter wri = PdfWriter.GetInstance(document, new FileStream(FilePathAndName, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Hello World"));
            document.Close();
        }

        private Paragraph CreateParagraph(Opdracht opdracht, List<Opdracht_Werknemer> opw, Werknemer werknemer, Prestatie prestatie, Prestatiesoort prestatiesoort, List<Artikel> artikels, List<Verbruikartikel> verbruikartikel)
        {
            string paragraph = "";
            paragraph += opdracht.omschrijving + " " + opdracht.locatie + "\n"
                + opw[0].datum.ToString() + "\n\n" + "Uitgevoerd door: " + werknemer.voornaam
                + " " + werknemer.naam + "\n" + prestatie.duur.ToString() + " " + prestatiesoort.omschrijving
                + "\n\n" + "Verbruikte artikels: ";
            foreach (Verbruikartikel a in verbruikartikel)
                paragraph += artikels[a.artikelid - 1].barcode + " " + artikels[a.artikelid - 1].omschrijving + " " + a.aantalGebruikt.ToString() + "\n";
            paragraph += "Voor akkoord \n\n";
            return new Paragraph(paragraph);
        }



    }
}
