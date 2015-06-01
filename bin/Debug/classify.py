import parse_data as p
import feature as f
import knn, nn

data  = []
label = []
# 
def setupClassifier(path):
	data  = []
	label = []
	l = p.getTrialList(path)
	for x in l:
		features = f.Features(x,path[-2])
		data.append(features.feature)
		if x.head.target == 'good':
			label.append(1)
		else:
			label.append(0)
	return knn.knn(data, label)

def testing_4():
	global clfback, clfarm, clfleg
	trial_list = p.TrialList()
	data = trial_list.readFile('test.txt')
	test_back = (f.Features(data,'k')).feature
	test_leg = (f.Features(data,'g')).feature
	test_arm = (f.Features(data,'m')).feature
	res1 = clfback.test(test_back,5)
	res2 = clfleg.test(test_leg,5)
	res3 = clfarm.test(test_arm,5)		
	s=0
	#s = 'back: ' + str(res1) + '	leg: '+ str(res2)+ '	arm: '+str(res3) + '\n'
	if res1 < -0.5:
		#s += "You're hunching your back.\n"
		s += 4
	if res2 < -0.5:
		#s += "You're crossing your leg.\n"
		s += 1
	if res3 < -0.5:
		#s += "Your arms are too close to your body.\n"
		s += 2
	if res1 > -0.5 and res2 > -0.5 and res3 > -0.5:
		#s += "You have perfect posture!\n"
		s=0
	return s
	

def testing_3():
	global clfback, clfarm, clfleg
	trial_list = p.TrialList()
	data = trial_list.readFile('test.txt')
	test_data = (f.Features(data)).feature
	res1 = clfback.test(test_data,10)
	res2 = clfleg.test(test_data,10)
	res3 = clfarm.test(test_data,10)		
	
	print res1, res2, res3
	minres = min(res1, res2, res3)
	if minres < -1.0:
		if minres == res1:
			s = "You're hunching your back.\n" 
		elif minres == res2:
			s = "You're crossing your leg.\n"
		else:
			s = "Your arms are too close to your body.\n"
	else:
		s = "You have perfect posture!\n"
	return s


clfback = setupClassifier('./data5.10/back/')
clfarm = setupClassifier('./data5.10/arm/')
clfleg = setupClassifier('./data5.10/leg/')


# clf2 = setupClassifier('./data/left/')
# clf3 = setupClassifier('./data/right/')
# clf4 = setupClassifier('./data/arm/')
# # # getTrainingData()
# # # classifier1 = knn.knn(data, label)
import cPickle
# # # # to serialize the object
# with open("clfback.dump", "wb") as output:
#     cPickle.dump(clfback, output, cPickle.HIGHEST_PROTOCOL)
# with open("clfarm.dump", "wb") as output:
#     cPickle.dump(clfarm, output, cPickle.HIGHEST_PROTOCOL)
# with open("clfleg.dump", "wb") as output:
#     cPickle.dump(clfleg, output, cPickle.HIGHEST_PROTOCOL)
# # with open("clf4.dump", "wb") as output:
# #     cPickle.dump(clf4, output, cPickle.HIGHEST_PROTOCOL)
# # to deserialize the object
# input = open("clfback.dump", "rb")	# back			
# clfback = cPickle.load(input) # protocol version is auto detected
# input = open("clfarm.dump", "rb") # left
# clfarm = cPickle.load(input) # protocol version is auto detected
# input = open("clfleg.dump", "rb") # right
# clfleg = cPickle.load(input) # protocol version is auto detected
# input = open("clf4.dump", "rb") # arm
# clf4 = cPickle.load(input) # protocol version is auto detected

res = testing_4()
print res
output_f = open('result.txt','w')
output_f.write(str(res))
output_f.close()


# def getTrainingData():
# 	l = p.trial_list.getTrials()
# 	for x in l:
# 		features = f.Features(x)
# 		data.append(features.feature)
# 		if x.head.target == 'good':
# 			label.append(1)
# 		else:
# 			label.append(0)

# def getTrainingData1():
# 	#print 'here'
# 	with open('training_data.txt','r') as df:
# 		for line in df:
# 			features = line.split(' ')
# 			features.pop()
# 			f2 = []
# 			for x in features:
# 				f2.append(float(x))
# 			#print f2
# 			data.append(f2)
# 	with open('training_label.txt','r') as df:
# 		for line in df:
# 			label.append(line[:-1])
# 	print data

# def testing():
# 	global classifier1
# 	trial_list = p.TrialList()
# 	data = trial_list.readFile('test.txt')
# 	test_data = (f.Features(data)).feature
# 	return classifier1.test(test_data,10)