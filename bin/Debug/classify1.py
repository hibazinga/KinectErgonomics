import parse_data as p
import feature as f

data  = []
label = []

def getTrainingData():
	l = p.trial_list.getTrials()
	for x in l:
		features = f.Features(x)
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)
	 	print data[-1], label[-1]
	# print len(data)
	# print len(label)

def setupClassifier(path):
	l = p.getTrialList(path)
	for x in l:
		features = f.Features(x)
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)

setupClassifier('./data5.9/leg/')
n = len(data)
# Run classifiers
import knn, nn

correct1, correct2 = 0, 0
for i in range(n):
	test_data  = data.pop(i)
	test_label = label.pop(i)

	#print 'at',i
	#kNN
	classifier1 = knn.knn(data, label)
#	print classifier1.test(test_data,10),',', test_label
	if classifier1.test(test_data,10) == test_label:
		correct1 += 1
	#print 'done with kNN'

	#NN
	# classifier2 = nn.nn(data, label)
	# if classifier2.test(test_data) == test_label:
	# 	correct2 += 1
	# print 'done with NN'

	data.insert(i, test_data)
	label.insert(i, test_label)
print 'kNN: ', correct1 * 1.0 / n
# print 'Neural Network: ', correct2 * 1.0 / n



