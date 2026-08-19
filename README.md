# 🛵 Deliveroo P.IVA Manager

## 📖 Descrizione
**Deliveroo P.IVA Manager** è un'applicazione desktop intuitiva e potente creata su misura per i rider (Deliveroo, Glovo, UberEats, ecc.) che operano con Partita IVA. Consente di tenere traccia di ogni sessione di lavoro, monitorare i guadagni lordi e netti, calcolare le spese vive come il carburante e stimare automaticamente l'accantonamento per le imposte e i contributi (INPS e aliquota fiscale configurabile).

## 🚀 Funzionalità Principali
- **Tracciamento Sessioni:** Inserisci facilmente incassi, chilometri percorsi, consegne effettuate e consumi del veicolo.
- **Calcolo Netto Reale:** L'app calcola il guadagno netto decurtando le spese di carburante, l'INPS e le altre tasse previste dal tuo regime fiscale.
- **Accantonamento Tasse:** Saprai sempre quanto mettere da parte per il Modello Redditi grazie alla stima automatica delle tasse in base all'imponibile.
- **Statistiche Avanzate:** Visualizza i report di riepilogo su base giornaliera, mensile o annuale.
- **Efficienza Lavorativa:** Un algoritmo calcola il "Voto di Efficienza" (da 1 a 10) della tua sessione basandosi sull'incidenza del carburante sul lordo generato.
- **Gestione Flessibile:** Aggiungi, aggiorna o elimina le sessioni di lavoro in qualsiasi momento (CRUD completo).
- **Dark / Light Mode:** Passa comodamente dal tema chiaro a quello scuro con un semplice clic.

<img width="999" height="514" alt="image" src="https://github.com/user-attachments/assets/e89b23b7-f7c7-4f37-9bb9-5be463bc26fa" />

## 🛠️ Tecnologie Utilizzate
- **Linguaggio:** VB.NET
- **Interfaccia Grafica (UI):** WPF (Windows Presentation Foundation)
- **Database Locale:** SQLite per l'archiviazione rapida e sicura dei dati

## 📦 Installazione e Avvio
1. Assicurati di avere [Visual Studio](https://visualstudio.microsoft.com/it/) installato sul tuo sistema con il carico di lavoro per "Sviluppo per desktop .NET".
2. Clona questo repository sul tuo computer:
   ```bash
   git clone https://github.com/tuo-username/deliveroo-piva.git
   ```
3. Apri il file della soluzione `deliveroo.slnx` o il progetto `deliveroo.vbproj` in Visual Studio.
4. Avvia la build e l'esecuzione premendo `F5`. L'app genererà in automatico un file di database SQLite (`deliveroo.db`) al primo avvio.

## 📝 Regimi Fiscali Supportati
L'app permette di configurare la propria aliquota fiscale per avere stime sempre precise sull'imponibile. I calcoli integrati sono tarati per il **Codice ATECO 82.99.99** (Altri servizi di supporto alle imprese nca), tipico per l'attività dei rider. Il sistema considera:
- **Coefficiente di redditività:** 67%.
- **Aliquota INPS (Gestione Separata):** 26,07%.
- **Aliquota Irpef/Sostitutiva:** Configurabile dall'utente anno per anno (es. 5% o 15% per Forfettario).

## 🤝 Contribuire
I contributi, le segnalazioni di bug e le pull request sono i benvenuti! Sentiti libero di aprire una **Issue** se noti comportamenti anomali o desideri suggerire nuove funzionalità.

---
*Sviluppato per semplificare la vita fiscale e contabile dei Rider professionisti.*
