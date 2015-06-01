from sklearn.neighbors import KNeighborsClassifier
from sklearn import svm
import parse_data as p
import feature as f

data  = []
label = []
classifiers = []
def getTrainingData():
	l = p.trial_list.getTrials()
	for x in l:
		features = f.Features(x)
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)
	 	#print data[-1], label[-1]
	# print len(data)
	# print len(label)
def testing():
	trial_list = p.TrialList()
	data = trial_list.readFile('test.txt')
	test_data = (f.Features(data)).feature
	res = ''
	#print test_data
	for clf in classifiers:
	#	print clf
		res += clf[0] + ' '
		res += str(clf[1].predict(test_data)[0]) + '\n'
	return res

getTrainingData()

import cPickle
input = open("neigh.dump", "rb")
neigh = cPickle.load(input) # protocol version is auto detected

# neigh = KNeighborsClassifier(n_neighbors=3)
# neigh.fit(data, label)
classifiers.append(('knn',neigh))

input = open("svm.dump", "rb")
clf = cPickle.load(input) # protocol version is auto detected


# clf = svm.SVC()
# clf.fit(data, label)
classifiers.append(('svm',clf))





print testing()