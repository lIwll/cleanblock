import datx


c = datx.City("17monipdb.datx")

with open("ips.txt", 'r') as ipsF:
	with open("output.txt", 'w') as outputF:
		for line in ipsF:
			ip = line.strip('\n')
			outputF.write(ip)
			outputF.write(",")
			outputF.write(c.find(ip)[0])
			outputF.write("\n")