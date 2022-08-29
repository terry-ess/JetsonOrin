import io
import datetime
import time

class Log:

	logfile = None
	name = None


	def Open(self):

		result = False
		if self.logfile:
			self.logfile.write("{0}\n".format(self.name))
			n = datetime.datetime.now()
			self.logfile.write("{0}/{1}/{2} {3}:{4}:{5}\n".format(n.month,n.day,n.year,n.hour,n.minute,n.second))
			self.WriteLine("")
			result = True
		return result		



	def Close(self):

		if self.logfile:
			self.WriteLine("closed")
			self.logfile.close()



	def WriteLine(self,line):
		if self.logfile:
			if (len(line) == 0):
				line = "\n"
			else:	
				t = int(time.perf_counter()*1000)
				line = "".join(("{0} ".format(t),line,"\n"))
			self.logfile.write(line)
			self.logfile.flush()


	def __init__(self,name):
		self.name = name
		n = datetime.datetime.now()
		name = "".join((name," {0}.{1}.{2} {3}.{4}.{5} .log".format(n.month,n.day,n.year,n.hour,n.minute,n.second)))
		self.logfile = open(name,"w")
