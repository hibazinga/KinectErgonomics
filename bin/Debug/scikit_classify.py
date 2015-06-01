from sklearn.neighbors import KNeighborsClassifier
from sklearn import svm
import parse_data as p
import feature as f
import numpy as np
import calc_stats
import pylab

data  = []
label = []
data_test = []
label_test = []

def getTrainingData(path):
	l = p.getTrialList(path)
	for x in l:
		features = f.Features(x, path[-2])
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)
	 	print data[-1], label[-1]
	# print len(data)
	# print len(label)

getTrainingData("./data5.10/back/")
# getTrainingData("./data5.10/arm/")
#getTrainingData("./data5.10/leg/")

def getTestingData(path):
	l = p.getTrialList(path)
	for x in l:
		features = f.Features(x, path[-2])
		data_test.append(features.feature)
		if x.head.target == 'good':
			label_test.append(1)
		else:
			label_test.append(0)
	 	print data_test[-1], label_test[-1]
	# print len(data)
	# print len(label)

getTestingData("./data5.9/back/")
# getTestingData("./data5.9/arm/")
#getTestingData("./data5.9/leg/")



# Run classifiers
neigh = KNeighborsClassifier(n_neighbors=3)
clf = svm.SVC(kernel='linear')
clf2 = svm.SVC(kernel='poly')

correct1, correct2, correct3 = 0, 0, 0
outputs1, outputs2, outputs3 = [],[],[]

neigh.fit(data, label)
clf.fit(data, label)
clf2.fit(data, label)
n = len(data_test)

for i in range(n):
	test_data  = data_test[i]
	test_label = label_test[i]

	#print 'at',i

	#kNN
	#classifier1 = knn.knn(data, label)
	#if classifier1.test(test_data,10) == test_label:
	#	correct1 += 1
	#print 'done with kNN'

	outputs1.append(neigh.predict(test_data))
	if test_label == neigh.predict(test_data):
		correct1 += 1

	#SVM
	outputs2.append(clf.predict(test_data))
	if test_label == clf.predict(test_data):
		correct2 += 1

	
	outputs3.append(clf2.predict(test_data))
	if test_label == clf2.predict(test_data):
		correct3 += 1

print 'kNN: ', correct1 * 1.0 / n
outputs1 = np.array(outputs1,dtype=float)
calculator1 = calc_stats.Stats_Calculator(outputs1, np.asarray(label_test))
calculator1.print_all_stats()

print 'SVM Linear: ', correct2 * 1.0 / n
outputs2 = np.array(outputs2,dtype=float)
calculator2 = calc_stats.Stats_Calculator(outputs2, np.asarray(label_test))
calculator2.print_all_stats()

print 'SVM Poly: ', correct3 * 1.0 / n
outputs3 = np.array(outputs3,dtype=float)
calculator3 = calc_stats.Stats_Calculator(outputs3, np.asarray(label_test))
calculator3.print_all_stats()

def getROCPoints(classifier):
	points = [[0,0],[1,1]]
	tmp = []
	if classifier == 0: # NN
		while (len(points) < 100):
			res = []
			run_ml()
			tp, fp, fn, tn = cal_tp_fp_fn_tn(classifier)
			if tp+fn > 0 and fp+tn>0:
				y = tp * 1.0 / (tp+fn)
				x = fp * 1.0 / (fp+tn)
				points.append([x,y])
	else:
		for k in range(15):
			res = []
			run_ml(k+1)
			tp, fp, fn, tn = cal_tp_fp_fn_tn(classifier)
			if tp+fn > 0 and fp+tn>0:
				y = tp * 1.0 / (tp+fn)
				x = fp * 1.0 / (fp+tn)
				points.append([x,y])

	points.sort()
	lx = [int(p[0]*100) for p in points]
	ly = [int(p[1]*100) for p in points]
	return lx,ly

def drawROC():
	pylab.figure()
	pylab.xlabel('False Positive Rate(%)')
	pylab.ylabel('True  Positive Rate(%)')
	pylab.title('ROC Curve')

	# draw NN
	x,y = getROCPoints(0)
	pylab.plot(x,y, c='red', label = "NN")

	# draw kNN
	x,y = getROCPoints(1)
	pylab.plot(x,y, c='blue', label = "kNN")

	pylab.legend()
	pylab.show()
