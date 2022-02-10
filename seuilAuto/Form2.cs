﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

using System.Runtime.InteropServices;
using Traitement;

namespace seuilAuto
{
    public partial class Form2 : Form
    {
        Form form1_;

        // Image puzzle et piece en variable globale
        Bitmap bmp_ref;
        Bitmap bmp_piece;
        Bitmap bmp_rogne;
        ClImage Img;

        public Form2(Form form1)
        {
            InitializeComponent();
            form1_ = form1;
        }

        // Fonction d'affichage de l'image d'attente. Exécution dans thread
        private void affiche_wait_image()
        {
            Image wait = Image.FromFile(@"..\\..\\wait.jpg");
            Bitmap bmp_wait = new Bitmap(wait);
            imageSeuillee.Image = bmp_wait;
        }

        // Fonction de détection de piece puzzle. Est éxécutée dans un thread
        private void traitement_bouton_go()
        {
            if (dudTraitSel.Text == "Sans rotation")
            {
                //imageSeuillee.Show();
                //valeurSeuilAuto.Show();


                Bitmap bmp_ref_copy = new Bitmap(bmp_ref); // Création d'une copie de l'image puzzle de référence
                Img = new ClImage(); // Initialisation d'une instance de classe ClImage pour appeller le wrapper

                // affectation des paramètres (a supprimer en faisant attention aux changement wrapper et dll, ne sert a rien)
                double[] parametres = { 0 }; // = new double[parametersTextBox.Lines.Length];
                                             //for (int i = 0; i < parametersTextBox.Lines.Length; i++)
                                             //   parametres[i] = Convert.ToDouble(parametersTextBox.Lines[i]);

                unsafe
                {
                    // Génération d'objets permettant de passer les data des images au wrapper
                    BitmapData bmpData = bmp_ref_copy.LockBits(new Rectangle(0, 0, bmp_ref_copy.Width, bmp_ref_copy.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    BitmapData bmpData_piece = bmp_piece.LockBits(new Rectangle(0, 0, bmp_piece.Width, bmp_piece.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    // Appel du wrapper pour traitement dans la dll
                    // Passage des données pour l'image puzzle de référence ET pour l'image piece
                    Img.traitementRognePtr(2, bmpData.Scan0, bmpData.Stride, bmp_ref_copy.Height, bmp_ref_copy.Width, parametres, 1, bmpData_piece.Scan0, bmpData_piece.Stride, bmpData_piece.Height, bmpData_piece.Width);

                    // ancien commentaire : 1 champ texte retour C++, le seuil auto
                    // Traitement terminé, libération des images
                    bmp_ref_copy.UnlockBits(bmpData);
                    bmp_piece.UnlockBits(bmpData_piece);
                }

                this.Invoke((MethodInvoker)delegate ()
                {
                    labelScore.Text = Img.objetLibValeurChamp(0).ToString() + " %";
                    labelScore2.Text = Img.objetLibValeurChamp(1).ToString() + " %";
                    labelScore.Show();
                    labelScore2.Show();
                });

                // Affichagr de l'image puzzle avec détection de pièce sur l'interface
                imageSeuillee.Image = bmp_ref_copy;

                // Traitement d'images
                ClImage clImageRogne = new ClImage();
                unsafe
                {
                    BitmapData bmpData_piece = bmp_piece.LockBits(new Rectangle(0, 0, bmp_piece.Width, bmp_piece.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    clImageRogne.RognagePtr(bmpData_piece.Scan0, bmpData_piece.Stride, bmp_piece.Height, bmp_piece.Width, 80, 255);
                    int Height_rogne = clImageRogne.getImgHauteur();
                    int Width_rogne = clImageRogne.getImgLargeur();
                    bmp_rogne = new Bitmap(Width_rogne, Height_rogne, PixelFormat.Format24bppRgb);
                    BitmapData bmpData_rogne = bmp_rogne.LockBits(new Rectangle(0, 0, Width_rogne, Height_rogne), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    clImageRogne.getImgdata(bmpData_rogne.Scan0, bmpData_rogne.Stride);
                    bmp_rogne.UnlockBits(bmpData_rogne);
                    bmp_piece.UnlockBits(bmpData_piece);
                }
                pbRogne.Image = bmp_rogne;
                
            }
            else if (dudTraitSel.Text == "Avec rotation")
            {
                Bitmap bmp_ref_copy = new Bitmap(bmp_ref); // Création d'une copie de l'image puzzle de référence
                Img = new ClImage(); // Initialisation d'une instance de classe ClImage pour appeller le wrapper

                unsafe
                {
                    // Génération d'objets permettant de passer les data des images au wrapper
                    BitmapData bmpData = bmp_ref_copy.LockBits(new Rectangle(0, 0, bmp_ref_copy.Width, bmp_ref_copy.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    BitmapData bmpData_piece = bmp_piece.LockBits(new Rectangle(0, 0, bmp_piece.Width, bmp_piece.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    // Appel du wrapper pour traitement dans la dll
                    Img.traitementRotPtr(1, bmpData_piece.Scan0, bmpData_piece.Stride, bmpData_piece.Height, bmpData_piece.Width, 80, 255);

                    // ancien commentaire : 1 champ texte retour C++, le seuil auto
                    // Traitement terminé, libération des images
                    bmp_ref_copy.UnlockBits(bmpData);
                    bmp_piece.UnlockBits(bmpData_piece);
                }

                pbRogne.Image = bmp_piece;
            }
            else if (dudTraitSel.Text == "Avec rotation (Pattern Matching)")
            {
                Bitmap bmp_ref_copy = new Bitmap(bmp_ref); // Création d'une copie de l'image puzzle de référence
                ClImage clImageRogne = new ClImage(); // Initialisation d'une instance de classe ClImage pour appeller le wrapper
                Img = new ClImage();
                ClImage ImgRot = new ClImage();
                unsafe
                {
                    // rotation
                    BitmapData bmpData_piece = bmp_piece.LockBits(new Rectangle(0, 0, bmp_piece.Width, bmp_piece.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    ImgRot.traitementRotPtr(1, bmpData_piece.Scan0, bmpData_piece.Stride, bmp_piece.Height, bmp_piece.Width, 80, 255);

                    // rognage
                    clImageRogne.RognagePtr(bmpData_piece.Scan0, bmpData_piece.Stride, bmp_piece.Height, bmp_piece.Width, 80, 255);
                    int Height_rogne = clImageRogne.getImgHauteur();
                    int Width_rogne = clImageRogne.getImgLargeur();
                    bmp_rogne = new Bitmap(Width_rogne, Height_rogne, PixelFormat.Format24bppRgb);
                    BitmapData bmpData_rogne = bmp_rogne.LockBits(new Rectangle(0, 0, Width_rogne, Height_rogne), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    clImageRogne.getImgdata(bmpData_rogne.Scan0, bmpData_rogne.Stride);
                    bmp_piece.UnlockBits(bmpData_piece);
                    bmp_rogne.UnlockBits(bmpData_rogne);
                    if (bmp_rogne.Width > bmp_rogne.Height)
                        bmp_rogne.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    //Pattern Matching
                    BitmapData bmpData = bmp_ref_copy.LockBits(new Rectangle(0, 0, bmp_ref_copy.Width, bmp_ref_copy.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    bmpData_rogne = bmp_rogne.LockBits(new Rectangle(0, 0, bmp_rogne.Width, bmp_rogne.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    Img.PatternMatchingPtr(bmpData.Scan0, bmpData.Stride, bmpData.Height, bmpData.Width, bmpData_rogne.Scan0, bmpData_rogne.Stride, bmpData_rogne.Height, bmpData_rogne.Width);
                    bmp_rogne.UnlockBits(bmpData_rogne);
                    bmp_ref_copy.UnlockBits(bmpData);
                }
                // Lecture des coordonnées auxquelles l'algo de pattern matching détecte l'image
                double coord_x = Img.objetLibValeurChamp(1);
                double coord_y = Img.objetLibValeurChamp(0);
                //MessageBox.Show(coord_x.ToString() + " " + coord_y.ToString());

                // Affichagr de l'image puzzle avec détection de pièce sur l'interface
                Color myRgbColor = new Color();
                myRgbColor = Color.Red;

                for (int i = -50; i < 50; i++)
                {
                    for (int j = -50; j < 50; j++)
                    {

                        for (int shift = -10; shift < 10; shift++)
                        {
                            if ((coord_x + i > 0) && (coord_x + i < bmp_ref_copy.Width) && (coord_y + shift > 0) && (coord_y + shift < bmp_ref_copy.Height))
                            {
                                bmp_ref_copy.SetPixel(Convert.ToInt32(coord_x + i), Convert.ToInt32(coord_y + shift), myRgbColor);
                            }

                        }

                        for (int shift = -10; shift < 10; shift++)
                        {
                            if ((coord_x + shift > 0) && (coord_x + shift < bmp_ref_copy.Width) && (coord_y + j > 0) && (coord_y + j < bmp_ref_copy.Height))
                            {
                                bmp_ref_copy.SetPixel(Convert.ToInt32(coord_x + shift), Convert.ToInt32(coord_y + j), myRgbColor);
                            }
                        }
                    }
                }
                this.Invoke((MethodInvoker)delegate ()
                {
                    labelScore.Text = Img.objetLibValeurChamp(2).ToString("N3");
                    labelScore.Show();
                });
                pbRogne.Image = bmp_rogne;
                imageSeuillee.Image = bmp_ref_copy;
            }

        }

        private void butForm1_Click(object sender, EventArgs e)
        {
            this.Hide();
            form1_.Show();
        }

        private void buttonOuvrir_Click(object sender, EventArgs e)
        {
            // On cache les scores
            

            if (ouvrirImage.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Récupération de l'image
                    Image img = Image.FromFile(ouvrirImage.FileName);
                    bmp_piece = new Bitmap(img);

                    // Ajustement de la taille du panel par rapport à l'image
                    imageDepart.Width = bmp_piece.Width;
                    imageDepart.Height = bmp_piece.Height;

                    // pour centrer image dans panel
                    if (imageDepart.Width < panel1.Width)
                        imageDepart.Left = (panel1.Width - imageDepart.Width) / 2;

                    if (imageDepart.Height < panel1.Height)
                        imageDepart.Top = (panel1.Height - imageDepart.Height) / 2;

                    // Affichage de l'image
                    imageDepart.Image = bmp_piece;

                    // Quand on affiche une nouvelle image, on cache l'ancienne image traitée pour garder une cohérence visuelle sur l'interface
                    //imageSeuillee.Hide();
                    imageSeuillee.Image = null;
                    pbRogne.Image = null;
                    //valeurSeuilAuto.Hide();
                    labelScore.Hide();
                    labelScore2.Hide();
                }
                catch
                {
                    MessageBox.Show("erreur !");
                }
            }
        }

        private void bSeuillageAuto_Click(object sender, EventArgs e)
        {
            // Affichage de l'image d'attente. Utilisation d'un thread sinon ne fonctionne pas
            if ((dudTraitSel.Text == "Sans rotation") || (dudTraitSel.Text == "Avec rotation"))
            {
                Thread th;
                th = new Thread(new ThreadStart(affiche_wait_image));
                th.Start();
            }

            // Lancement du traitement de la piece. Lancement dans un autre thread, sinon cela plante l'interface et n'affiche pas l'image d'attente
            Thread th_trait;
            th_trait = new Thread(new ThreadStart(traitement_bouton_go));
            th_trait.Start();
            
        }

        private void button_puzzle_Click(object sender, EventArgs e)
        {
            if (ouvrirImage.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Initialisations
                    Image img = Image.FromFile(ouvrirImage.FileName);
                    bmp_ref = new Bitmap(img);

                    pictureBox2.Image = bmp_ref; // Affichage de l'image

                    //imageSeuillee.Hide();
                    //valeurSeuilAuto.Hide();
                }
                catch
                {
                    MessageBox.Show("erreur !");
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void imageSeuillee_Click(object sender, EventArgs e)
        {

        }

        private void pbRogne_Click(object sender, EventArgs e)
        {

        }
    }
}
