
# coding: utf-8

# In[1]:

import pytesseract
from PIL import Image
import nltk
import shutil
import os


# In[2]:

def f1(str):
        token = nltk.word_tokenize(str);
        tagged = nltk.pos_tag(token);
        #print(tagged);
        return tagged


# In[3]:

text= pytesseract.image_to_string(Image.open('images/test1.png')) 
print text


# In[4]:

file  = f1(text)
print(file)


# In[5]:

text_file = open("Output.txt", "w")
text_file.write("%s" % file)
text_file.close()


# In[6]:

srcfile = 'Output.txt'
dstroot = '/home/student/Desktop/share/Soft/Soft/bin/Release'

assert not os.path.isabs(srcfile)
dstdir =  os.path.join(dstroot, os.path.dirname(srcfile))

#os.makedirs(dstdir) 
shutil.copy(srcfile, dstdir)


# In[ ]:













