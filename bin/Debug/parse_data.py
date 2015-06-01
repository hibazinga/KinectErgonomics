#
# Parser for posture data
#
# Author: Guanya Yang
# SID: 204378262
# CS 239 - Spring 2015
#
import sys

sys.path.append("C:\\Program Files (x86)\\IronPython 2.7\\Lib")
import os, re

class Position: # a single position, i.e. head, wrist, hip
	def __init__(self, id, target, label, coordinates):
		self.id = id
		self.target = target
		self.label = label
		self.x 	= float(coordinates[0])
		self.y = float(coordinates[1])
		self.z = float(coordinates[2])

	def printData(self):
		print self.id, self.target,'for', self.label, ': ('+ str(self.x) +','+ str(self.y)+','+ str(self.z),')'


class Trial:
	def __init__(self, position_set):
		self.position_set = position_set
		self.head = position_set[0]
		self.shoulder_center   = position_set[1]
		self.shoulder_left  = position_set[2]
		self.shoulder_right   = position_set[3]
		self.spine   = position_set[4]
		self.hip_center = position_set[5]
		self.hip_left = position_set[6]
		self.hip_right = position_set[7]
		self.elbow_left = position_set[8]
		self.wrist_left = position_set[9]
		self.hand_left = position_set[10]
		self.elbow_right = position_set[11]
		self.wrist_right = position_set[12]
		self.hand_right = position_set[13]
		self.knee_left = position_set[14]
		self.ankle_left = position_set[15]
		self.knee_right = position_set[16]
		self.ankle_right = position_set[17]

	def print_data(self):
		for position in self.position_set:
			position.printData()


class TrialList:
	# Extract position data from files
	def __init__(self):
		self.trial_list = []

	def readFiles(self, path):
		files = os.listdir(path)

		for filename in files:
			fileReader = open(path + filename,'rU')
			# the target (1 or 0) is in the name of the file
			number = filename.split('_')[0]
			target = re.search("_[a-z]*.",filename).group(0).replace("_","").replace(".","")

			positions = []
			position_list = []
			# print filename
			for line in fileReader:
				if line == '--------------------------------------\n' :
					continue
				
				# print line
				try:
					label = re.search(".*:  ",line).group(0).replace(":  ","")
					x = re.search("X: .* ,Y",line).group(0).replace("X: ","").replace(',Y','').strip()
					y = re.search("Y: .* ,",line).group(0).replace("Y: ","").replace(',','').strip()
					z = re.search("Z: .*\s*",line).group(0).replace("Z: ","").strip()
					#print label, x, y, z
					# Create a new position
					position = Position(number, target, label, [x,y,z])
					position_list.append(position)

					if label == 'Ankle Right': #end of trial
						# Add a new data point for trial
						data_pt = Trial(position_list)
						self.trial_list.append(data_pt)
						position_list = []
				except:
					break

	def readFile(self, filepath):
		fileReader = open(filepath,'rU')
		positions = []
		position_list = []

		for line in fileReader:
			# print line
			if line[1] == '-':
				continue
			tokens = line.split(' ')
			
			label = re.search(".*:  ",line).group(0).replace(":  ","")
			x = tokens[-5]
			y = tokens[-3]
			z = tokens[-1]
			#print label, x, y, z
			# Create a new position
			position = Position('', '', '', [x,y,z])
			position_list.append(position)

			if label == 'Ankle Right': 
				return Trial(position_list)

	def getTrials(self):
		return self.trial_list


def getTrialList(path):
	trial_list = TrialList()
	trial_list.readFiles(path)	
	return trial_list.getTrials()

# def main():
# 	path = "./data/back"
# 	trial_list = TrialList()
# 	trial_list.readFiles(path)
# 	#position_list.plot_bp()

# if __name__ == "__main__":
# 	main()