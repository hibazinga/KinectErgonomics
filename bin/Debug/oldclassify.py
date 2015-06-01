import parse_data as p
import feature as f
import knn, nn

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

def getTrainingData1():
	#print 'here'
	with open('training_data.txt','r') as df:
		for line in df:
			features = line.split(' ')
			features.pop()
			f2 = []
			for x in features:
				f2.append(float(x))
			#print f2
			data.append(f2)
	with open('training_label.txt','r') as df:
		for line in df:
			label.append(line[:-1])
	print data

def testing():
	global classifier1
	trial_list = p.TrialList()
	data = trial_list.readFile('test.txt')
	test_data = (f.Features(data)).feature
	return classifier1.test(test_data,10)	

def setupClassifier(path):
	data  = []
	label = []
	l = p.getTrialList(path)
	for x in l:
		features = f.Features(x)
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)
	return knn.knn(data, label)

def testing_4():
	global clf1, clf2, clf3, clf4
	trial_list = p.TrialList()
	data = trial_list.readFile('test.txt')
	test_data = (f.Features(data)).feature
	res1 = clf1.test(test_data,10)
	res2 = clf2.test(test_data,10)	
	res3 = clf3.test(test_data,10)	
	res4 = clf4.test(test_data,10)
	
	str1, str2, str3 = "","",""
	if res1 == 0:
		str1 = "You're hunching your back"
	if res2 + res3 == 1:
		str2 = "You're leaning to the left/right."
	if res4 == 0:
		str3 = "Your arms are too close to your body."

	if res1+res2+res3+res4 == 4:
		return "You have perfect posture!\n-------------------------------------------\n"
	return str1+"\n"+str2+"\n"+str3+"\n-------------------------------------------\n"


clf1 = setupClassifier('./data5.9/back/')
clf2 = setupClassifier('./data5.9/arm/')
clf3 = setupClassifier('./data5.9/leg/')


# clf2 = setupClassifier('./data/left/')
# clf3 = setupClassifier('./data/right/')
# clf4 = setupClassifier('./data/arm/')
# # getTrainingData()
# # classifier1 = knn.knn(data, label)
import cPickle
# # # to serialize the object
# with open("clf1.dump", "wb") as output:
#     cPickle.dump(clf1, output, cPickle.HIGHEST_PROTOCOL)
# with open("clf2.dump", "wb") as output:
#     cPickle.dump(clf2, output, cPickle.HIGHEST_PROTOCOL)
# with open("clf3.dump", "wb") as output:
#     cPickle.dump(clf3, output, cPickle.HIGHEST_PROTOCOL)
# with open("clf4.dump", "wb") as output:
#     cPickle.dump(clf4, output, cPickle.HIGHEST_PROTOCOL)
# # to deserialize the object
# input = open("clf1.dump", "rb")	# back			
# clf1 = cPickle.load(input) # protocol version is auto detected
input = open("clf2.dump", "rb") # left
clf2 = cPickle.load(input) # protocol version is auto detected
input = open("clf3.dump", "rb") # right
clf3 = cPickle.load(input) # protocol version is auto detected
input = open("clf4.dump", "rb") # arm
clf4 = cPickle.load(input) # protocol version is auto detected

res = testing_4()
print res
output_f = open('result.txt','w')
output_f.write(str(res))
output_f.close()


