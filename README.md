# RabbitHole
<p>AES-256 encrypted file archive with any number of hidden volumes for plausible deniability.<img        src="https://github.com/eflite/RabbitHole/blob/master/rabbitHoleLogo2.png" align="right">
</p>

## Introduction
Inspired by TrueCrypt and similar software, this command line tool offers serious encryption for your files through a command line tool for Windows. It's uses [BouncyCastle 1.8.1](https://en.wikipedia.org/wiki/Bouncy_Castle_(cryptography)), an acclaimed crypto library providing strong encryption. Because your file archive is first populated with random data, any encrypted volumes you create inside are indistinguishable from the random data. Thus there is no way to ascertain whether you have 0, 1, 2 or 100 volumes within your archive. This gives you [plausible deniability](https://en.wikipedia.org/wiki/Plausible_deniability#Use_in_cryptography), so that any adversary cannot prove or be sure that any encrypted volume exists. A typical way to use this is to create at least 2 volumes, one that you can safely decrypt and reveal should you be forced to do so, and one that you can plausibly deny exists. 


