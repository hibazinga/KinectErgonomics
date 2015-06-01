import random

class nn:
	lambd = 0.15
	num_of_loop = 300

	def __init__(self, training, labels, w = [],th = 0):
		if len(w) == 0:
			self.train_data = training
			self.labels = labels
			self.featureNum = len(training[0])
			self.threshold = random.random()
			self.w = [0] * self.featureNum
			self.initWeight()
			self.train()
		else:
			self.w = w
			self.featureNum = len(w)
			self.threshold = th

	def initWeight(self):
		for i in range(self.featureNum):
			self.w[i] = random.random()
			if random.random() < 0.5:
				self.w[i] = - self.w[i]
# [-146859.23723603095, 51366.489270999744, -29172.8316247607, 51366.153643961734, 10316.77101700324]
# 611.023498511

# [-266832.5192973095, 99714.34392117932, -52974.174215337036, 99712.60687456501, 17604.89184322042]
# 1198.20973921

# [-266831.75163017836, 99713.26674597104, -52975.02336542216, 99713.75542450407, 17605.36316565741]
# 1198.06898244



	def train(self):
		for l in range(self.num_of_loop):
			for d in range(len(self.train_data)):
				data  = self.train_data[d]
				label = self.labels[d] 
				y = self.classify(data)
				
				for i in range(self.featureNum):
			 		self.w[i] -= self.lambd * (y - label) * data[i]
				self.threshold += self.lambd * (y - label)
	
	def classify(self, data):
		y = 0
		for i in range(self.featureNum):
			y += self.w[i] * data[i]
		if y > self.threshold:
			return 1
		else:
			return 0

	def test(self,data_list):
		return self.classify(data_list)

