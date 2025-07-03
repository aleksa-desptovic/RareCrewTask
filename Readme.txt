Neki podaci nisu ispravni (EndTimeUtc je pre StarTimeUtc ili je polje EmployeeName null).
U slucaju sa vremenom moze se koristiti pretpostavka da je zamenjeno pocetno i krajnje vreme i onda koristiti apsolutnu vrednost razlike.
U oba slucaja sam postavio warning u konzolu sa ID-em da bi se pronasla greska (Angular) i zanemario te sate.
U slucaju kada je DeletedOn != null pretostavka je da su ti sati obrisani i da se ne racunaju u konacan zbir.
Takodje pretpostavka je da poziv get vraca samo rezultate iz jednog meseca.
