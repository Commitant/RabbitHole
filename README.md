# RabbitHole
<p>AES-256 encrypted file archive with any number of hidden volumes for plausible deniability.<img        src="https://github.com/eflite/RabbitHole/blob/master/rabbitHoleLogo3.png" align="right"/>
</p>

* Encrypted file archive
* AES 256 (Rijndael block cipher)
* Bouncy Castle, trusted crypto
* Any number of hidden volumes
* Encrypted volumes indistinguishable from random data
* Plausible deniability 
* Very small code base, easily inspected and audited
* Open source, free software (GNU GPL v3)

## Getting started
Get started by downloading the [latest release](https://github.com/eflite/RabbitHole/releases/latest). For maximum security you may opt to download the source code and compile it yourself. 

## How to use
Check out the [How To Use](https://github.com/eflite/RabbitHole/wiki/How-To-Use)

## Introduction
Inspired by TrueCrypt and similar software, this application offers serious encryption for your files through a command line tool for Windows. It's uses [BouncyCastle 1.8.1](https://en.wikipedia.org/wiki/Bouncy_Castle_(cryptography)), an acclaimed crypto library providing strong encryption. Because your file archive is first populated with random data, any encrypted volumes you create inside are indistinguishable from the random data. Thus there is no way to ascertain whether you have 0, 1, 2 or 20 volumes within your archive. This gives you [plausible deniability](https://en.wikipedia.org/wiki/Plausible_deniability#Use_in_cryptography), so that an adversary cannot prove or be sure that any encrypted volume exists. A typical way to use this is to create at least 2 volumes, one that you can safely decrypt and reveal should you be forced to, and one that contains your real secrets and which existence you can plausibly deny. For a cryptography tool to have any value we believe is has to be open source software, so users and experts can inspect the code and make sure no vulnerabilities or back doors exist. That is why this project is released under the open source GPLv3 lisence. For maximum security, download the source code, download the [Bouncy Castle crypto library for C#](https://www.nuget.org/packages/BouncyCastle.Crypto.dll/) and compile it yourself.   

## How it works
When you create a new file archive with RabbitHole you specify the file name and total archive size, and a new file is created and filled with random data. The Sha512 Bouncy Castle random generator, a cryptographically secure pseudo random function [(a CSPRF)](https://en.wikipedia.org/wiki/Cryptographically_secure_pseudorandom_number_generator) is used and seeded with entropy collected from the keyboard through keystroke timings. When you later create logical volumes within the archive, there is no way of distinguishing these from garbage random data from the random generator. There is no header information within the archive specifying the number or the sizes of the logical volumes. The only thing that identifies a volume is the password you chose for it, and this is obviously not written to the file. Rather, when opening an archive and entering the password for the volume you want to open, the application tries to read data at different positions and decrypt it with your password. It will first try to read at the start of the archive with an offset calculated from the hash of the password, then it will try at half way through the file plus the offset, then at half way of the remaing part of the file plus the offset etc, etc. It will repeat this process until a volume decrypts successfully or the file is exhausted. 
As the start positions are halved for every volume you create, the allocated space for each volume is also halved. This means that if you create an archive of size N bytes, your first volume can occupy n/2 bytes, your second volume can occupy n/4 bytes, your third can occupy n/8 bytes etc. Because each volume requires at least 1068 bytes, a 1 MB archive allows for 10 volumes, a 10 MB archive allows for 14 volumes, and a 100 MB archive allows for 17 volumes etc.

![diagram1](https://github.com/eflite/RabbitHole/blob/master/RabbitHoleDiagram1.png)

A volume can can contain any number of files with unique names. To keep the code base small, a volume cannot contain sub folders, but this can be remedied by first organazing your file structure in a ZIP file (or any other archive) before adding it to your RabbitHole archive. If you try to add more files to a volume than there is allocated space for by default, you will be prompted for confirmation. As the software itself does not know whether you have created more volumes than the one you are currently accessing, it will allow exhausting this threshold if you confirm your action. 

Example: You have created an archive of a total of 100 MB, and created 3 volumes within. By default, the first archive will have allocated 50 MB, the second will have 25 MB, and the third will have 12.5 MB. However, because you have no more volumes after the third one, you can safely use the remaining space for the third volume, allowing it 25 MB in total. So in effect, only the last volume can exceed its default allocated space. The software will allow you to exceed the default allocation if confirmed, but if you do this on any volume other than the last, all successive volumes will be corrupted and forever lost. 

A single volume is constructed in the following way:
First there's a 0-1024 byte preamble of random data, effectively an offset calculated from the hash of the password. Then there is 32 bytes for the [initialization vector](https://en.wikipedia.org/wiki/Initialization_vector) (IV), followed by 4 bytes describing the length of the following encrypted data. Lastly the encrypted data follows.  

![diagram2](https://github.com/eflite/RabbitHole/blob/master/rabbitHoleDiagram2.png)

## Q and A
Q: How secure is RabbitHole?

A: That depends on the password you choose for your volumes. If you use a [strong password](https://en.wikipedia.org/wiki/Password_strength#Common_guidelines) it should be impossible to crack by brute force, taking millions of years on super computers. A weak password will compromise security whichever cryptographic algorithm is used. If you plan on creating 2 or more volumes inside your archive for plausible deniability, at least ensure you are using a strong password for the second volume presumably containing your real secrets. That being said, all crypto tools need code review and audit. If you're a crypto expert or you know one, you're very much welcome to inspect the open source code. If you would like to contribute to the project, please let us know. 
***
Q: There's other tools for encrypting files and archives out there. Why should I use RabbitHole?

A: We think the combination of properties make the tool interesting. The open source nature and small code base makes the application very easy to review, audit and verify. Plausible deniability through multiple volumes, strong encryption through AES 256 and Bouncy Castle makes it safe and secure. 
***
Q: What is plausible deniablity, and why do I need it?

A: Plausible deniablity, or is this case deniable cryptography, describes encryption techniques where the existence of an encrypted file or message is deniable in the sense that an adversary cannot prove that the plaintext data exists. In many countries around the world, such as the UK, [you will land in jail](https://www.theverge.com/2017/5/17/15653786/rabbani-encryption-password-charged-terrorism-uk-airport) for not giving up your password if demanded by law enforcement, even if you're not suspected of any wrongdoing. In the US, you can be [held in prisen indefinitely](https://www.theregister.co.uk/2017/08/30/ex_cop_jailed_for_not_decrypting_data/) for not decrypting your data. Let's say an adversary ceases your file archive, and demands that you provide the password. If you claim that there's zero volumes in the archive, or that you've forgotten the password, most would probably not believe you. However, if you have created multiple volumes, you could safely give up the password to your safe volume, keeping your real secrets safe. And even if your adversary suspects the existence of multiple volumes, he has no way to know for sure, he has no way to find out, and he has no way to prove it.
***
Q: How many volumes should I create within one archive?

A: If you don't need plausible deniability you can create just a single volume, and use the entire archive space for your encrypted files. If you want plausible deniablity you should create 2 or more volumes. How many is a matter of preference, but 2 is sufficient for plausible deniability. The idea is that you have at least one volume which you can give up if pressed to do so, and at least one hidden volume which existence is impossible to establish or prove. But more volumes require more passwords to remember, and the password for your hidden volume(s) should be totally different from your dummy volume(s).  
***
Q: How should I create my passwords?

A: If you want plausible deniability you need at least 2 volumes, and two COMPLETELY different passwords. If ever pressed by a competent adversary to reveal the password to your dummy volume, you can be sure they will try bruteforcing your potential hidden volumes by creating variations over that password. You can get away with having weak passwords for your dummy volumes though, if it doesn't matter if they are brute force cracked. Example: Your two dummy volumes could have the passwords someSimplePassword1 and someSimplePassword2, while your hidden volume has the password ¤€&theSupr%SecrtPa$$w├rd.
The general formula for possible passwords is [possible characters]^[length], where ^ is "to the power of". This means increasing your password's length will do more for password security than adding more special characters, but a strong password should also contain numbers, upper case letters and special characters. RabbitHole supports using alt-codes, which allows you to use the entire 8 bit character space when inputing characters for your password. This is done by holding down the Alt-key and entering a 3 digit number on your keyboards' numerical keypad. 

***
Q: I've lost my password(s), can you help?

A: No, you're thoroughly out of luck. No one in the whole world can help you. 

