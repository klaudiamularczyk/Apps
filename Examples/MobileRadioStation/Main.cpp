#include <uhd/utils/thread_priority.hpp>
#include <uhd/utils/safe_main.hpp>
#include <uhd/usrp/multi_usrp.hpp>
#include <uhd/exception.hpp>
#include <uhd/types/tune_request.hpp>
#include <boost/program_options.hpp>
#include <boost/format.hpp>
#include <boost/thread.hpp>
#include <iostream>
#include <complex>
#include <fstream>
#include <stdio.h>
#include <termios.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>
#include <wiringPi.h>
#include <wiringSerial.h>
#include <vector>
using namespace std;
vector<complex<float>> buff;


void Buff()
{
  ifstream fileTo;
  string line;
  vector<float> componentI
  vector<float> componentQ
  vector<float> signal
  vector<float> integratedSignal
  const float factor= 2.135;
  vector<float> compIover, compQover;
  int i=0;
  
  fileTo.open("/home/pi/Downloads/FileToSend.txt");
  while(!fileTo.eof())
  {
      getline(fileTo,line);
      if (line.length() !=0)
      {
         sygnal.insert(signal.end(),stof(line));
         i++;
      }
    else{}
  }
    
    fileTo.close();
    integratedSignal.insert(integratedSignal.end(),signal[0]);
    for (int i=1;i< signal.size();i++)
    {
      integratedSignal.insert(integratedSignal.end(),signal[i]+integratedSignal[i-1]);
    }
  ofstream file;
  file.open("/home/pi/Downloads/Scalkowany.txt", ios::app);
  for (int i=0;i< integratedSignal.size();i++)
  {
    file<<integratedSignal[i]<< '\n';
  }
  file.close();
  
  for (int i=0;i< integratedSignal.size();i++)
  {
    componentI.insert( componentI.end(),sin(integratedSignal[i] * factor));
    componentQ.insert( componentQ.end(),cos(integratedSignal[i] * factor));
  }
  
ofstream files, file2;

files.open("/home/pi/Downloads/SkladowaI.txt", ios::app);
file2.open("/home/pi/Downloads/SkladowaQ.txt", ios::app);
for (int i=0;i< componentI.size();i++)
{
    files<< componentI[i]<< '\n';
    file2<< componentQ[i]<< '\n';
}
files.close();
file2.close();

float aI,bI,aQ,bQ;
compIover.insert(compIover.end(),componentI[0]);
compQover.insert(compQover.end(),componentQ[0]);
for (int i=0;i<componentI.size()-1;i++)
{
    aI=componentI[i];
    bI=componentI[i+1];
    aQ=componentQ[i];
    bQ=componentQ[i+1];
    float cI=(bI-aI)/27.0;
    float cQ=(bQ-aQ)/27.0;
for (int j=1; j<27 ; j++)
{
    compIover.insert(compIover.end(),componentI[i]+j*cI);
    compQover.insert(compQover.end(),componentQ[i]+j*cQ);
}
    compIover.insert(compIover.end(), componentI[i+1]);
    compQover.insert(compQover.end(), componentQ[i+1]);
}
ofstream myfile,my;

myfile.open("/home/pi/Downloads/InterI.txt", ios::app);
my.open("/home/pi/Downloads/InterQ.txt", ios::app);

for (int i=0;i<compIover.size();i++)
{
  complex<float> temp(compIover[i], compQover[i]);
  buff.insert(buff.end(),temp);
  myfile<<compIover[i]<< '\n';
  my<<compQover[i]<< '\n';
}

myfile.close();
my.close();
}

int UHD_SAFE_MAIN(int argc,char *argv[])
{
    uhd::set_thread_priority_safe();
    string frequency, gains, adress;
    cout << "Podaj adres urzadzenia USRP:" << endl;
    cin >> adress;
    cout << "Podaj czestotliwosc pracy urzadzenia USRP:" << endl;
    cin >> frequency;
    cout << "Podaj wartosc wzmocnienia:" << endl;
    cin >> gains;
    double freq,gain;
    size_t check_freq = frequency.find(",");
    
    if(check_freq == string::npos)
    {
      freq = stod(frequency);
    }
    else
    {
      frequency.replace(check_freq,1,".");
      freq = stod(frequency);
    }
    gain= stod(gains);
    double rate(198413);
    
    string device_args("addr="+adress);
    uhd::usrp::multi_usrp::sptr usrp = uhd::usrp::multi_usrp::make(device_args);
    usrp->set_tx_freq(freq);
    usrp->set_tx_gain(gain);
    usrp->set_tx_rate(rate);
    
    cout << "Ustalona czestotliwosc: " << usrp->get_tx_freq()/1e6 << " MHz " << endl;
    cout << "Ustalona wartosc wzmocnienia: " << usrp->get_tx_gain() << endl;
    cout << "Ustalona czestotlwosc probkowania: " << usrp->get_tx_rate()/1e3 << " kHz" << endl;
    
    uhd::stream_args_t stream_args("fc32");
    uhd::tx_streamer::sptr tx_stream = usrp->get_tx_stream(stream_args);
    
    Buff();

    uhd::tx_metadata_t md;
    md.start_of_burst = true;
    md.end_of_burst = false;
    md.has_time_spec = false;
    size_t num_tx_samps = tx_stream->send(&buff.front(), buff.size(), md);
    cout <<"Wyslano " << num_tx_samps << " probek." <<endl;
    remove("/home/pi/Downloads/FileToSend.txt");
return EXIT_SUCCESS;
}
