#
# Functions for calculating statistic measurements
#
# Author: Guanya Yang
# SID: 204378262
# CS 260 - Winter 2015
#

import parse_data
import numpy as np
import pylab

class Stats_Calculator:
	def __init__(self, outputs, targets):
		self.outputs = outputs
		self.targets = targets
		if len(outputs) != len(targets):
			print "Unequal number of outputs to targets"

		# Calculate true/false positives/negatives
		self.tp, self.fp, self.tn, self.fn = 0,0,0,0
		for i in range(len(outputs)):
			if outputs[i] == 1 and targets[i] == 1:
				self.tp += 1
			elif outputs[i] == 1 and targets[i] == 0:
				self.fp += 1
			elif outputs[i] == 0 and targets[i] == 0:
				self.tn += 1
			elif outputs[i] == 0 and targets[i] == 1:
				self.fn += 1

		print 'TP: ',self.tp
		print 'FP: ',self.fp
		print 'TN: ',self.tn
		print 'FN: ',self.fn

	def accuracy(self):
		return (float(self.tp)+self.tn)/(self.tp+self.fp+self.tn+self.fn)

	def sensitivity(self):
		return float(self.tp)/(self.tp+self.fn)

	def specificity(self):
		return float(self.tn)/(self.tn+self.fp)

	def precision(self):
		return float(self.tp)/(self.tp+self.fp)

	def recall(self):
		return float(self.tp)/(self.tp+self.fn)

	def f_measure(self):
		return 2*(self.precision()*self.recall())/(self.precision()+self.recall())

	# Inputs must be in the form of numpy arrays
	def plot_ROC(self,tp_list,fp_list):
		pylab.figure()
		x, y = [], []

		for t in tp_list:
			y.append(t)
		for f in fp_list:
			x.append(f)
		x.sort()
		y.sort()
		pylab.plot(x, y, 'bo')
		pylab.plot([0,1])

		pylab.axis((0,1,0,1))
		pylab.xlabel('False Positive Rate')
		pylab.ylabel('True Positive Rate')
		title = "ROC Curve"
		pylab.title(title)
		#pylab.show()
		pylab.savefig("./" + title+'.png')


	# Inputs (self.outputs, self.targets) must be in the form of numpy arrays
	def confusion_matrix(self):
		# Add the inputs that match the bias node
		nclasses = 2
		#norm_outputs = np.where(self.outputs>0.5,1,0)

		cm = np.zeros((nclasses,nclasses))
		# for i in range(nclasses):
		# 	for j in range(nclasses):
		# 		cm[i,j] = np.sum(np.where(self.outputs==i,0,1)*np.where(self.targets==j,0,1))
		cm[0,0] = self.tp
		cm[0,1] = self.fn
		cm[1,0] = self.fp
		cm[1,1] = self.tn
		print "Confusion matrix is:"
		print cm
		print "Percentage Correct: ",np.trace(cm)/np.sum(cm)*100

	def print_all_stats(self):
		print 'Accuracy: ',self.accuracy()
		print 'Sensitivity: ',self.sensitivity()
		print 'Specificity: ',self.specificity()
		print 'Precision: ',self.precision()
		print 'Recall: ',self.recall()
		print 'F-measure: ',self.f_measure()

		self.confusion_matrix()


def main():
	calc = Stats_Calculator([],[])
	#tp/(tp+fn)
	#fp/(fp+tn)
	#tp_list = np.array([0.35,0.45,0.5,0.65,0.7,0.6,0.5,0.7])
	#fp_list = np.array([0,0.2,0.1666667,0.263158,0.4,0.15789,0.368421,0.47368421])
	tp_list = np.array([1,0.94737,0.894737,0.947368,0.68421,0.8421,0.57895,0.63158,0.78947])
	fp_list = np.array([0.35,0.5,0.45,0.65,0.6,0.65,0.5,0.45,0.7])
	calc.plot_ROC(tp_list, fp_list)

if __name__ == "__main__":
	main()