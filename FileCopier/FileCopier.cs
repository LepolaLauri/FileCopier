using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FileCopier
{
    /// <summary>
    /// Tiedoston kopiointi luokka. Käyttää sisäisesti Binary/Reader/Writer -luokkia.
    /// </summary>
    /// Property BufferSize - bufferin koko (oletus 1024)
    /// Property InteractiveMode - hash-juoksu consolissa
    /// Property GetCopyTime - palauttaa kopioinnissa käytetyn ajan (TimeSpan)
    /// Ver 1.0
    class FileCopy
    {
        private string _sourceFile;
        private string _destinationFile;

        /// <summary>
        /// Bufferin koko. (Oletus 1024)
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// Interaktiivinen moodi. Hash-rullaus konsolille.
        /// </summary>
        public bool InteractiveMode { get; set; }
        /// <summary>
        /// Tiedoston kopiointiin käytetty aika. 
        /// </summary>
        public TimeSpan GetCopyTime { get; private set; }
        /// <summary>
        /// Kopioitava tiedosto (polkuineen) (SISÄINEN)
        /// </summary>
        /// <returns>Kopioitava tiedosto (polkuineen)</returns>
        public string SourceFileWithPath() { return _sourceFile; }
        private void SourceFileWithPath(string FileWithPath)
        {
            if (FileWithPath == null)
                throw new ArgumentNullException("Source file not defined", FileWithPath);
            if (!File.Exists(FileWithPath))
                throw new System.IO.FileNotFoundException("Source file not found", FileWithPath);
            FileInfo fi = new FileInfo(FileWithPath);
            if (fi.Length == 0)
                throw new ArgumentNullException("Source file has empty", FileWithPath);
            _sourceFile = FileWithPath;
        }
        /// <summary>
        /// Kopio tiedostosta (polkuineen) (SISÄINEN)
        /// </summary>
        /// <returns>Kopio tiedostosta (polkuineen)</returns>
        public string DestinationFileWithPath() { return _destinationFile; }
        private void DestinationFileWithPath(string FileWithPath)
        {
            if (FileWithPath == null)
                throw new ArgumentNullException("Destination file not defined", FileWithPath);
            if (File.Exists(FileWithPath))
                throw new System.IO.FileNotFoundException("Destination file is exists", FileWithPath);
            _destinationFile = FileWithPath;
        }

        /// <summary>
        /// FileCopier luokan alustus.
        /// </summary>
        /// <param name="sourceFile">Kopioitava tiedosto</param>
        /// <param name="destinationFile">Uusi tiedostonimi</param>
        public FileCopy(string sourceFile, string destinationFile)
        {
            SourceFileWithPath(sourceFile);                 // Kopioitava tiedosto
            DestinationFileWithPath(destinationFile);       // Uusi tiedostonimi

            this.InteractiveMode = false;                   // Konsolikäyttöön
            this.BufferSize = 1024;                         // Oletus koko bufferille
        }
        /// <summary>
        /// Kopioi tiedoston.
        /// </summary>
        /// <returns>Onnistuiko kopiointi.</returns>
        public bool Copy()
        {
            bool retval = false;

            try
            {
                retval = CopyFile();
            }
            catch (Exception) { throw; }

            return retval;
        }
        /// <summary>
        /// Kopioi tiedoston. (SISÄINEN)
        /// </summary>
        /// <returns>Onnistuiko kopiointi.</returns>
        private bool CopyFile()
        {
            // Onko kopiointi onnistunut
            bool returnValue = false;

            // Määritellään kopioitavan tiedoston koko
            FileInfo fi = new FileInfo(_sourceFile);
            long sourceFileLength = fi.Length;
            fi = null;

            // Bufferi johon tiedot kopioidaan muistiin. Koko voidaan määritellä. (this.BufferSize)
            byte[] buffer = new byte[this.BufferSize]; ;

            // Lasketaan kuinka monta kierrosta on kopiointia tehtävä (tiedoston sekä bufferin koko vaikuttaa tähän)
            long blocks = (int)Math.Floor((decimal)(sourceFileLength / this.BufferSize));
            // Lasketaan jääkö lopuksi vajaa bufferi
            int restBytes = (int)(sourceFileLength - (blocks * this.BufferSize));

            // Instanssi kelloa varten
            Stopwatch stopWatch = new Stopwatch();

            // Interaktiivista moodia varten
            long hashCount = blocks / 50;
            char[] nox = { '[', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ']' };

            try
            {
                // Käynnistetään kello
                stopWatch.Start();

                using (BinaryWriter bw = new BinaryWriter(File.Open(_destinationFile, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    using (BinaryReader br = new BinaryReader(File.Open(_sourceFile, FileMode.Open, FileAccess.Read)))
                    {

                        int currentIndex = 0;

                        // Interaktiivisen moodin apumuuttujat 
                        long nix = hashCount;
                        int counter = 1;
                        int col = 0;
                        int row = 0;

                        if (this.InteractiveMode)
                        {
                            Console.Write($"Copying file.. ");
                            col = Console.CursorLeft;
                            row = Console.CursorTop;
                        }

                        // Loopataan kunnes kaikki kierrokset (blocks) on tehty, lukuunottamatta jos jää vajaa kierros
                        while (blocks > 0)
                        {
                            // Vähennetään kierroslaskuria
                            blocks -= 1;

                            // Luetaan kopioitavasta tiedostosta sisältö bufferiin
                            br.Read(buffer, 0, this.BufferSize);

                            // Kirjoitetaan kopioitavaan tiedostoon bufferin sisältö
                            bw.Write(buffer);
                            bw.Flush();

                            // Määritellään luettava kohta
                            currentIndex += this.BufferSize;
                            br.BaseStream.Position = currentIndex;

                            // Interaktiivisessa moodissa tulostetaan konsoliin liikkuva hash-palkki
                            if (this.InteractiveMode)
                            {
                                nix--;
                                if (nix == 0)
                                {
                                    if (counter < 11)
                                    {
                                        Console.SetCursorPosition(col, row);
                                        if (counter > 2)
                                            nox.SetValue('-', counter - 1);
                                        nox.SetValue('#', counter);
                                        Console.Write(nox);
                                        counter++;
                                    }
                                    else
                                        counter = 1;

                                    nix = hashCount;
                                }
                            }
                        }

                        // Vajaan kierroksen (ei kokonainen bufferi) luku ja kirjoitus
                        if (restBytes > 0)
                        {
                            // Määritetään paljonko vajaa  bufferi tarvitsee kokoa
                            Array.Resize(ref buffer, restBytes);

                            // Luetaan kopioitavasta tiedostosta sisältö bufferiin
                            buffer = br.ReadBytes(restBytes);

                            // Kirjoitetaan kopioitavaan tiedostoon bufferin sisältö
                            bw.Write(buffer);
                            bw.Flush();

                            if (this.InteractiveMode)
                                Console.Write($"#");
                        }
                        buffer = null;
                    }
                }

                // Määritellään kopioidun tiedoston koko
                fi = new FileInfo(_destinationFile);
                long destinationFileSize = fi.Length;
                fi = null;

                // Pysäytetään kello
                stopWatch.Stop();

                // Tarkistetaan ovat kopioitava ja kopioitu tiedosto saman kokoiset
                if (sourceFileLength != destinationFileSize)
                    returnValue = false;
                else
                {
                    // Tallennetaan kello
                    this.GetCopyTime = stopWatch.Elapsed;
                    returnValue = true;
                }

            }
            catch (Exception) { returnValue = false; throw; }

            finally
            {
            }

            return returnValue;
        }
    }
}
