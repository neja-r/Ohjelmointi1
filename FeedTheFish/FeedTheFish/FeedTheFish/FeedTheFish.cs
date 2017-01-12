using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
///@author Nea Räisänen
///@version 10.12.2013
/// <summary>
/// Kalapeli, jossa väistellään isoja kaloja ja syödään pikkukaloja
/// </summary>
public class FeedTheFish : PhysicsGame
{
    Vector nopeusYlos = new Vector(0, 700);
    Vector nopeusAlas = new Vector(0, -700);
    Vector nopeusVasen = new Vector(0, 0);
    Vector nopeusOikea = new Vector(500, 0);

    Vector nopeusVastaantulija = new Vector(-100, 0);
    Vector nopeusVastaantulijaReset = new Vector(-100, 0);

    private PhysicsObject pelaaja;

    GameObject liikkuvatausta1;
    GameObject liikkuvatausta2;
    IntMeter pisteLaskuri;
    DoubleMeter nalkaLaskuri;

    private static Image[] kalaKuvat = new Image[] { LoadImage("rfisu1"), LoadImage("rfisu2"), LoadImage("rfisu3"), LoadImage("rfisu4"), LoadImage("rfisu5"), 
                                                     LoadImage("DUNPI"), LoadImage("haikala"), LoadImage("hauki"), LoadImage("kalmari"), LoadImage("lohikala"), LoadImage("petokala1") };

    /// <summary>
    /// Kutsutaan aliohjelmia, jotka luovat kentän ja pelihahmot ja ohjaimet
    /// </summary>
    public override void Begin()
    {
        LuoKentta();
        LisaaNappaimet();
    }


    /// <summary>
    /// Luodaan pelikenttä
    /// </summary>
    public void LuoKentta()
    {
        liikkuvatausta1 = new GameObject(LoadImage("taustakuva"));
        liikkuvatausta2 = new GameObject(LoadImage("taustakuva"));
        //double x = RandomGen.NextDouble(-400, 400);
        //double y = RandomGen.NextDouble(-400, 400);
        Level.Background.CreateGradient(Color.Navy, Color.SkyBlue);
        LuoPistelaskuri();
        LuoNalkaLaskuri();

        pelaaja = LisaaPelaaja(game: this, x: -50, y: 0);

        Camera.ZoomToLevel();
        Camera.Move(nopeusOikea);

        LuoTaustakuvat();

        Timer ajastinKalat = new Timer();
        ajastinKalat.Interval = 1.0; // Kuinka usein ajastin "laukeaa" sekunneissa
        ajastinKalat.Timeout += delegate
        {
            LisaaKala(this, pelaaja.X + 800, RandomGen.NextDouble(pelaaja.Y - 400, pelaaja.Y + 400), "ruoka",
             RandomGen.NextDouble(pelaaja.Height - 60, pelaaja.Height - 10), RandomGen.NextDouble(pelaaja.Width - 60, pelaaja.Width - 10)); // ruokakalat ovat pelaajaa pienempiä
            LisaaKala(this, pelaaja.X + 800, RandomGen.NextDouble(pelaaja.Y - 400, pelaaja.Y + 400), "pahis", RandomGen.NextDouble(pelaaja.Height + 10, pelaaja.Height + 80),
              RandomGen.NextDouble(pelaaja.Width + 10, pelaaja.Width + 80)); // petokalat ovat pelaajaa suurempia
        }; // Aliohjelma, jota kutsutaan eri parametreillä 1.0 sekunnin välein
        ajastinKalat.Start(); // Ajastin käynnistetään

        Timer ajastinnalka = new Timer();
        ajastinnalka.Interval = 2.0;
        ajastinnalka.Timeout += NalkaLisaantyy; //kutsutaan nälkää lisäävää aliohjelmaa tietyn ajan välein
        ajastinnalka.Start();

    }


    /// <summary>
    /// Luodaan kentälle taustakuvat.
    /// </summary>
    void LuoTaustakuvat()
    {
        liikkuvatausta1.Width = Screen.Width + 500;
        liikkuvatausta1.Height = Screen.Height + 400;
        Add(liikkuvatausta1, -3); // lisätään taustakuva alimmalle tasolle näyttöön

        liikkuvatausta2.Width = Screen.Width + 500;
        liikkuvatausta2.Height = Screen.Height + 400;
        liikkuvatausta2.Y = liikkuvatausta1.Position.Y;
        liikkuvatausta2.Left = liikkuvatausta1.Right;
        Add(liikkuvatausta2, -3); // lisätään taustakuva alimmalle tasolle näyttöön

    }


    /// <summary>
    /// Luodaan pistelaskuri 
    /// </summary>
    public void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 100;
        pisteNaytto.Y = Screen.Top - 80;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.Transparent;
        pisteNaytto.Title = "Pisteet";
        pisteNaytto.BindTo(pisteLaskuri); //sidotaan pistelaskuri pistenäyttöön
        Add(pisteNaytto);
    }


    /// <summary>
    /// Luodaan nälkäpalkki ruudun oikeaan yläreunaan
    /// </summary>
    void LuoNalkaLaskuri()
    {
        nalkaLaskuri = new DoubleMeter(10);
        nalkaLaskuri.MaxValue = 10;
        nalkaLaskuri.LowerLimit += AloitaAlusta; // aloitetaan alusta, jos nälkälaskuri laskee nollille

        ProgressBar nalkaPalkki = new ProgressBar(150, 30);
        nalkaPalkki.X = Screen.Right - 150;
        nalkaPalkki.Y = Screen.Top - 50;
        nalkaPalkki.Color = Color.Fuchsia; // palkin taustaväri
        nalkaPalkki.BarColor = Color.DarkCyan; // palkin väri
        nalkaPalkki.BorderColor = Color.Black; // reunan väri
        nalkaPalkki.BindTo(nalkaLaskuri);
        Add(nalkaPalkki); // lisätään palkki, johon on sidottu nälkälaskuri
    }


    /// <summary>
    /// Aliohjelma, joka luo pelaajan.
    /// </summary>
    /// <param name="game">Peli, johon pelaaja luodaan</param>
    /// <param name="x">Pelaajan x-koordinaatti</param>
    /// <param name="y">Pelaajan y-koordinaatti</param>
    /// <returns></returns>
    public PhysicsObject LisaaPelaaja(Game game, double x, double y)
    {
        Image pelaajanKuva = LoadImage("rumakala");
        pelaaja = PhysicsObject.CreateStaticObject(100, 80, Shape.Ellipse);
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Image = pelaajanKuva;

        AddCollisionHandler(pelaaja, "pahis", PelaajaOsuuPetokalaan);
        AddCollisionHandler(pelaaja, "ruoka", PelaajaOsuuRuokakalaan);
        game.Add(pelaaja);
        return pelaaja;
    }

    /// <summary>
    /// Luodaan pelaajaa vastaan uiva kala
    /// </summary>
    /// <param name="game">Peli, johon kala luodaan</param>
    /// <param name="x">kalan x-koordinaatti</param>
    /// <param name="y">kalan y-koordinaatti</param>
    /// <param name="tag">kalan tagi</param>
    /// <param name="korkeus">kalan korkeus</param>
    /// <param name="leveys">kalan leveys</param>
    /// <returns>kala</returns>
    public PhysicsObject LisaaKala(Game game, double x, double y, string tag, double korkeus, double leveys)
    {
        PhysicsObject kala = new PhysicsObject(leveys, korkeus, Shape.Ellipse);
        kala.X = x;
        kala.Y = y;

        int a = RandomGen.NextInt(kalaKuvat.Length); // arvotaan satunnainen luku 0-kuvataulukon maksimipituus
        for (int i = 0; i < kalaKuvat.Length; i++) // etsitään a mukainen indeksi
        {
            if (i == a)
                kala.Image = kalaKuvat[i]; // ruokakalan kuvaksi tulee taulukossa i. alkio.
        }
        kala.IgnoresCollisionResponse = true;
        kala.Tag = tag;
        kala.LifetimeLeft = TimeSpan.FromSeconds(3.0);
        kala.Velocity = nopeusVastaantulija;
        game.Add(kala);
        return kala;

    }


    /* /// <summary>
     /// Aliohjelma luo syötävän kalan
     /// </summary>
     /// <param name="game">Peli, johon kala luodaan</param>
     /// <param name="x">Kalan x-koordinaatti</param>
     /// <param name="y">Kalan y-koordinaatti</param>
     /// <returns></returns>
     public PhysicsObject LisaaRuokakala(Game game, double x, double y)
     {
         PhysicsObject ruokakala = new PhysicsObject(RandomGen.NextDouble(30, pelaaja.Width - 50), RandomGen.NextDouble(30, pelaaja.Height - 50), Shape.Ellipse);
         ruokakala.X = x;
         ruokakala.Y = y;
       
         int a = RandomGen.NextInt(ruokakalaKuvat.Length); // arvotaan satunnainen luku 0-kuvataulukon maksimipituus
         for(int i = 0 ; i < ruokakalaKuvat.Length ; i++){ //etsitään a mukainen indeksi
         if (i == a)
             ruokakala.Image = ruokakalaKuvat[i]; //ruokakalan kuvaksi tulee taulukossa i. alkio.
         }
         ruokakala.IgnoresCollisionResponse = true;
         ruokakala.Tag = "ruoka";
         ruokakala.LifetimeLeft = TimeSpan.FromSeconds(3.0);
         ruokakala.Velocity = nopeusVastaantulija;
         game.Add(ruokakala);
         return ruokakala;
     }
     */

    /* /// <summary>
     /// Aliohjelma luo petokalan, jota pitää väistellä
     /// </summary>
     /// <param name="game">Peli, johon petokala luodaan</param>
     /// <param name="x">Petokalan x-koordinaatti</param>
     /// <param name="y">Petokalan y-koordinaatti</param>
     /// <returns></returns>
      public PhysicsObject LisaaPetokala(Game game, double x, double y)
     { 
         PhysicsObject petokala = new PhysicsObject(RandomGen.NextDouble(pelaaja.Width, pelaaja.Width + 300), RandomGen.NextDouble(pelaaja.Height, pelaaja.Height + 200), Shape.Ellipse);
         petokala.X = x;
         petokala.Y = y;
       
         int a = RandomGen.NextInt(petokalaKuvat.Length); //arvotaan satunnainen kokonaisluku 0 - petokalakuvataulukon max-pituus
         for (int i = 0; i < petokalaKuvat.Length; i++)   // etsitään a:n mukainen indeksi
         {
             if (i == a)
             petokala.Image = petokalaKuvat[i]; //petokalan kuvaksi tulee petokalakuvataulukon i. alkio
         }

         petokala.Tag = "pahis";
         petokala.LifetimeLeft = TimeSpan.FromSeconds(3.0);
         petokala.IgnoresCollisionResponse = true;
         petokala.Velocity = nopeusVastaantulija;
         game.Add(petokala);
         return petokala;
     }
     */


    /// <summary>
    /// Aliohjelmassa määritellään pelinäppäimet
    /// </summary>
    public void LisaaNappaimet()
    {
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Up, ButtonState.Down, AsetaNopeusY, "Liiku ylöspäin", pelaaja, nopeusYlos);
        Keyboard.Listen(Key.Down, ButtonState.Down, AsetaNopeusY, "Liiku alas", pelaaja, nopeusAlas);
        Keyboard.Listen(Key.Left, ButtonState.Pressed, AsetaNopeusX, "Pysähdy", pelaaja, nopeusVasen);
        Keyboard.Listen(Key.Right, ButtonState.Pressed, AsetaNopeusX, "Liiku oikealle", pelaaja, nopeusOikea);
        //Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeusX, "Pysähdy", pelaaja, nopeusVasen);

        Keyboard.Listen(Key.P, ButtonState.Pressed, Pause, "Pysäyttää pelin");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Avustus");
    }


    /// <summary>
    /// Asetetaan pelaajalle nopeus y-suunnassa
    /// </summary>
    /// <param name="pelaaja">Pelihahmo jolla liikutaan</param>
    /// <param name="nopeus">Pelihahmon nopeus</param>
    void AsetaNopeusY(PhysicsObject pelaaja, Vector nopeus)
    {
        pelaaja.Velocity = new Vector(pelaaja.Velocity.X, nopeus.Y);

    }


    /// <summary>
    /// Asetetaan pelaajalle nopeus x-suunnassa
    /// </summary>
    /// <param name="pelaaja">Pelihahmo jolla liikutaan</param>
    /// <param name="nopeus">Pelihahmon nopeus</param>
    void AsetaNopeusX(PhysicsObject pelaaja, Vector nopeus)
    {
        pelaaja.Velocity = new Vector(nopeus.X, pelaaja.Y);

    }


    /// <summary>
    /// Kun pelaaja osuu ruokakalaan, "syödään" ruokakala, lisätään piste pistelaskuriin, kasvatetaan nälkälaskuria
    /// sekä vastaan tulevien kalojen nopeutta
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="ruokakala">Ruokakala johon törmätään</param>
    void PelaajaOsuuRuokakalaan(PhysicsObject pelaaja, PhysicsObject ruokakala)
    {
        ruokakala.Destroy();
        pisteLaskuri.Value += 1;
        nalkaLaskuri.Value += 2;
        nopeusVastaantulija = nopeusVastaantulija + nopeusVastaantulija / 50;
    }


    /// <summary>
    /// Pienennetään nälkälaskurin arvoa
    /// </summary>
    void NalkaLisaantyy()
    {
        nalkaLaskuri.Value -= 1;
    }


    /// <summary>
    /// Aliohjelmassa tuhotaan pelaaja ja kutsutaan AloitaAlusta-aliohjelmaa
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="petokala">Petokala johon törmätään</param>
    void PelaajaOsuuPetokalaan(PhysicsObject pelaaja, PhysicsObject petokala)
    {
        pelaaja.Destroy();
        AloitaAlusta();
    }


    /// <summary>
    /// Tyhjennetään kenttä ja luodaan se uudestaan
    /// </summary>
    void AloitaAlusta()
    {
        nopeusVastaantulija = nopeusVastaantulijaReset; //palautetaan nopeus alkuperäiseksi
        ClearAll();
        LuoKentta();
        LisaaNappaimet();

    }


    /// <summary>
    /// Laitetaan kamera seuraamaan pelaajaa. Estetään pelaajaa "poistumasta" ruudusta.
    /// Laitetaan taustakuvat "pyörimään" taustalla.
    /// </summary>
    /// <param name="time">Sisältää tiedon ajasta</param>
    protected override void Update(Time time)
    {
        base.Update(time);
        if (pelaaja == null) return;

        if (pelaaja.Y < Level.Bottom) pelaaja.Y = Level.Bottom; // estetään pelaajaa poistumasta ruudusta y-suunnassa
        if (pelaaja.Y > Level.Top) pelaaja.Y = Level.Top;

        Camera.Position.X = pelaaja.Position.X + 300;


        double width = Screen.Width;
        double vasenreuna = Camera.X - width / 2;


        if (liikkuvatausta1.Right < vasenreuna) liikkuvatausta1.Left = liikkuvatausta2.Right; // pyöritetään kahta taustakuvaa
        if (liikkuvatausta2.Right < vasenreuna) liikkuvatausta2.Left = liikkuvatausta1.Right;
    }


}
