import datx


c = datx.City("17monipdb.datx")

print(c.find("36.80.145.153"))
#print(c.find("8.8.8.258"))
#print(c.find("255.255.255.255"))

d = datx.District("17monipdb.datx")
print(d.find("110.54.224.23"))
#print(d.find("256.181.153.22"))

d = datx.BaseStation("17monipdb.datx")
print(d.find("110.54.224.23"))