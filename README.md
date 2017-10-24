# RabbitHole
<p>AES-256 encrypted file archive with any number of hidden volumes for plausible deniability.<img        src="https://github.com/eflite/RabbitHole/blob/master/rabbitHoleLogo3.png" align="right">
</p>

* Encrypted file archive
* AES 256 (Rijndael block cipher)
* Bouncy castle, trusted crypto
* Any number of hidden volumes
* Encrypted volumes indistinguishable from random data
* Plausible deniability 
* Very small code base, easily inspected and audited
* Open source, free software

## Introduction
Inspired by TrueCrypt and similar software, this command line tool offers serious encryption for your files through a command line tool for Windows. It's uses [BouncyCastle 1.8.1](https://en.wikipedia.org/wiki/Bouncy_Castle_(cryptography)), an acclaimed crypto library providing strong encryption. Because your file archive is first populated with random data, any encrypted volumes you create inside are indistinguishable from the random data. Thus there is no way to ascertain whether you have 0, 1, 2 or 100 volumes within your archive. This gives you [plausible deniability](https://en.wikipedia.org/wiki/Plausible_deniability#Use_in_cryptography), so that an adversary cannot prove or be sure that any encrypted volume exists. A typical way to use this is to create at least 2 volumes, one that you can safely decrypt and reveal should you be forced to do so, and one that contains your real secrets and which you can plausibly deny exists. For a cryptography tool to have any value we believe is has to be open source software, so users and experts can inspect the code and make sure no vulnerabilities or back doors exists. That is why this project is released under the open source GPLv3 lisence. For maximum security, download the source code, download the [Bouncy Castle crypto library for C#](https://www.nuget.org/packages/BouncyCastle.Crypto.dll/) and compile it yourself.   

## How it works
When you create a new file archive with RabbitHole you specify the file name and total archive size, and a new file is created and filled with random data. The Sha512 random generator, a cryptographically secure pseudo random function [(a CSPRF)](https://en.wikipedia.org/wiki/Cryptographically_secure_pseudorandom_number_generator) is used and seeded with entropy collected from the keyboard though keystroke timings. 
