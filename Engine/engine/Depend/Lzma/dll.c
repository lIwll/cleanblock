#ifdef WIN32
#ifdef _USRDLL
#define LZMA_API _declspec(dllexport)
#else
#define LZMA_API _declspec(dllimport)
#endif
#elif defined ANDROID
#include <jni.h>
#define LZMA_API JNIEXPORT
#else
#define LZMA_API
#endif
#include <stdlib.h>
#include <stdio.h>

#include "LzmaDec.h"
#include "LzmaEnc.h"

LZMA_API SRes LzmaDecJpzOrCopyKpk(const char *srcFilePath, int srcOffset, int srcLength, const char *distFilePath);
LZMA_API SRes LzmaDecompressJpz(const char *srcFilePath, int srcOffset, int srcLength, const char *distFilePath);
LZMA_API SRes LzmaCompressJpz(const char *srcFilePath, const char *dstFilePath);
LZMA_API int GetLzmaDecompressJpzTotalSize();
LZMA_API int GetLzmaDecompressJpzCurrentSize();

void *LZMAAlloc(void *p, size_t size)
{
	return malloc(size);
}
void LZMAFree(void *p, void *address)
{
	free(address);
}

typedef struct
{
	ISeqOutStream funcTable;
	FILE *pFile;
} LZMAWriteStruct;

static size_t LZMAWrite(void *pp, const void *data, size_t size)
{
	LZMAWriteStruct *p = (LZMAWriteStruct*)pp;
	return fwrite(data, 1, size, p->pFile);
}

typedef struct
{
	ISeqInStream funcTable;
	FILE *pFile;
} LZMAReadStruct;

SRes LZMARead(void *pp, void *buf, size_t *size)
{
	LZMAReadStruct *p = (LZMAReadStruct*)pp;
	*size = fread(buf, 1, *size, p->pFile);
	return SZ_OK;
}

#define IN_BUFFER_SIZE 8388608
#define OUT_BUFFER_SIZE 12582912

static volatile int currentOutSize;
static volatile int totalOutSize;

SRes LZMAProgress(void *p, UInt64 inSize, UInt64 outSize)
{
	currentOutSize = (int)inSize;
	return SZ_OK;
}

int GetLzmaDecompressJpzTotalSize()
{
	return totalOutSize;
}

int GetLzmaDecompressJpzCurrentSize()
{
	return currentOutSize;
}

SRes LzmaCompressJpz(const char *srcFilePath, const char *dstFilePath)
{
	ISzAlloc alloc;
	alloc.Alloc = LZMAAlloc;
	alloc.Free = LZMAFree;
	CLzmaEncHandle handle = LzmaEnc_Create(&alloc);
	if (!handle)
		return SZ_ERROR_MEM;
	struct _CLzmaEncProps props;
	LzmaEncProps_Init(&props);
	props.dictSize = 1 << 23;
	props.pb = 2;
	props.lc = 3;
	props.lp = 0;
	props.numThreads = 1;
	props.writeEndMark = 0;
	LzmaEncProps_Normalize(&props);
	SRes res = LzmaEnc_SetProps(handle, &props);
	if (res == SZ_OK)
	{
#ifdef WIN32
		FILE *pSrcFile = NULL;
		FILE *pDstFile = NULL;
		fopen_s(&pSrcFile, srcFilePath, "rb");
		fopen_s(&pDstFile, dstFilePath, "wb");
#else
		FILE *pSrcFile = fopen(srcFilePath, "rb");
		FILE *pDstFile = fopen(dstFilePath, "wb");
#endif
		Byte header[20];
		header[0] = 'J';
		header[1] = 'P';
		header[2] = 'Z';
		header[3] = 1;
		header[4] = 0;
		header[5] = 0;
		header[6] = 0;
		SizeT propSize = LZMA_PROPS_SIZE;
		res = LzmaEnc_WriteProperties(handle, header + 7, &propSize);
		fseek(pSrcFile, 0L, SEEK_END);
		int srcLength = ftell(pSrcFile);
		totalOutSize = srcLength;
		fseek(pSrcFile, 0L, SEEK_SET);
		int j = 0;
		for (int i = 7 + LZMA_PROPS_SIZE; i < 20; i++, j++)
		{
			int v = (srcLength >> (8 * j)) & 255;
			header[i] = (Byte)v;
		}
		fwrite(header, 1, 20, pDstFile);
		LZMAWriteStruct outStream;
		outStream.funcTable.Write = &LZMAWrite;
		outStream.pFile = pDstFile;
		LZMAReadStruct inStream;
		inStream.funcTable.Read = &LZMARead;
		inStream.pFile = pSrcFile;
		ICompressProgress progress;
		progress.Progress = &LZMAProgress;
		if (res == SZ_OK)
		{
			res = LzmaEnc_Encode(handle, &outStream.funcTable, &inStream.funcTable, &progress, &alloc, &alloc);
		}
		fclose(pDstFile);
		fclose(pSrcFile);
	}
	LzmaEnc_Destroy(handle, &alloc, &alloc);
	return res;
}

SRes LzmaDecJpzOrCopyKpk(const char *srcFilePath, int srcOffset, int srcLength, const char *distFilePath)
{
	totalOutSize = 0;
	currentOutSize = 0;
#ifdef WIN32
	FILE *pf = NULL;
	fopen_s(&pf, srcFilePath, "rb");
#else
	FILE *pf = fopen(srcFilePath, "rb");
#endif
	if (pf == NULL)
	{
		return SZ_ERROR_INPUT_EOF;
	}
	Byte header[20];
	while (1)
	{
		fseek(pf, srcOffset, SEEK_SET);
		if (fread(header, 1, 20, pf) != 20)
		{
			break;
		}
		if (header[0] != 'J' || header[1] != 'P' || header[2] != 'Z')
		{
			break;
		}
		if (header[3] != 1 || header[4] != 0 || header[5] != 0 || header[6] != 0)
		{
			break;
		}
		fclose(pf);
		return LzmaDecompressJpz(srcFilePath, srcOffset, srcLength, distFilePath);
	}
	if (srcLength <= 0)
	{
		fseek(pf, 0L, SEEK_END);
		srcLength = ftell(pf);
	}
	totalOutSize = srcLength;
	fseek(pf, srcOffset, SEEK_SET);
#ifdef WIN32
	FILE *pOutf = NULL;
	fopen_s(&pOutf, distFilePath, "wb");
#else
	FILE *pOutf = fopen(distFilePath, "wb");
#endif
	if (pOutf == NULL)
	{
		fclose(pf);
		return SZ_ERROR_OUTPUT_EOF;
	}
	int read = 0;
	Byte *pDst = malloc(OUT_BUFFER_SIZE);
	do
	{
		read = (int)fread(pDst, 1, srcLength<OUT_BUFFER_SIZE ? srcLength : OUT_BUFFER_SIZE, pf);
		if (read > 0)
		{
			srcLength -= read;
			fwrite(pDst, 1, read, pOutf);
			currentOutSize += read;
		}
	} while (read > 0 && srcLength > 0);
	free(pDst);
	fclose(pf);
	fclose(pOutf);
	totalOutSize = 0;
	return srcLength > 0 ? read : 0;
}

SRes LzmaDecompressJpz(const char *srcFilePath, int srcOffset, int srcLength, const char *distFilePath)
{
	currentOutSize = 0;
	totalOutSize = 0;
	CLzmaDec p;
	SRes res;

	ISzAlloc alloc;
	alloc.Alloc = LZMAAlloc;
	alloc.Free = LZMAFree;
#ifdef WIN32
	FILE *pf = NULL;
	fopen_s(&pf, srcFilePath, "rb");
#else
	FILE *pf = fopen(srcFilePath, "rb");
#endif
	if (pf == NULL)
	{
		return SZ_ERROR_INPUT_EOF;
	}

#ifdef WIN32
	FILE *pOutf = NULL;
	fopen_s(&pOutf, distFilePath, "wb");
#else
	FILE *pOutf = fopen(distFilePath, "wb");
#endif
	
	if (pOutf == NULL)
	{
		fclose(pf);
		return SZ_ERROR_OUTPUT_EOF;
	}

	if (srcLength <= 0)
	{
		fseek(pf, 0L, SEEK_END);
		srcLength = ftell(pf) - srcOffset;
	}
	fseek(pf, srcOffset, SEEK_SET);
	srcLength -= 20;
	Byte header[20];
	if (fread(header, 1, 20, pf) != 20)
	{
		fclose(pf);
		fclose(pOutf);
		return SZ_ERROR_INPUT_EOF;
	}
	if (header[0] != 'J' || header[1] != 'P' || header[2] != 'Z')
	{
		fclose(pf);
		fclose(pOutf);
		return SZ_ERROR_UNSUPPORTED;
	}
	if (header[3] != 1 || header[4] != 0 || header[5] != 0 || header[6] != 0)
	{
		fclose(pf);
		fclose(pOutf);
		return SZ_ERROR_UNSUPPORTED;
	}
	long outSize = 0;
	int j = 0;
	for (int i = 7 + LZMA_PROPS_SIZE; i < 20; i++, j++)
	{
		int v = header[i];
		if (v < 0)
		{
			fclose(pf);
			fclose(pOutf);
			return SZ_ERROR_UNSUPPORTED;
		}
		outSize |= ((long)(Byte)v) << (8 * j);
	}
	totalOutSize = outSize;
	Byte *pSrc = malloc(IN_BUFFER_SIZE);
	if (pSrc == NULL)
	{
		fclose(pf);
		fclose(pOutf);
		totalOutSize = 0;
		return SZ_ERROR_MEM;
	}
	Byte *pDst = malloc(OUT_BUFFER_SIZE);
	if (pDst == NULL)
	{
		fclose(pf);
		fclose(pOutf);
		free(pSrc);
		totalOutSize = 0;
		return SZ_ERROR_MEM;
	}
	LzmaDec_Construct(&p);
	RINOK(LzmaDec_Allocate(&p, header + 7, LZMA_PROPS_SIZE, &alloc));
	LzmaDec_Init(&p);
	int srcLeft = srcLength;
	int outLeft = outSize;
	int inBufferSize = 0;
	int inBufferPos = 0;
	SizeT outBufferPos = 0;
	SizeT inProcessed = 0;
	ELzmaStatus status;
	while (1) {
		if (inBufferSize == inBufferPos)
		{
			inBufferSize = (int)fread(pSrc, 1, IN_BUFFER_SIZE < srcLeft ? IN_BUFFER_SIZE : srcLeft, pf);
			srcLeft -= inBufferSize;
			inBufferPos = 0;
			//printf("Read File, left:%d\n", srcLeft);
		} 
		
		outBufferPos = OUT_BUFFER_SIZE < outLeft ? OUT_BUFFER_SIZE : outLeft;
		inProcessed = inBufferSize - inBufferPos;
		//printf("inBufferPos: %d, inProcessed: %d, outBufferPos: %d\n", inBufferPos, (int)inProcessed, (int)outBufferPos);
		res = LzmaDec_DecodeToBuf(&p, pDst, &outBufferPos, pSrc + inBufferPos, &inProcessed, LZMA_FINISH_ANY, &status);
		//printf("res: %d, status: %d, outBufferPos %d\n", (int)res, (int)status, (int)outBufferPos);
		inBufferPos += (int)inProcessed;
		outLeft -= (int)outBufferPos;
		currentOutSize += (int)outBufferPos;
		//printf("outLeft: %d, inBufferPos:%d, inBufferSize:%d\n", (int)outLeft, (int)inBufferPos, (int)inBufferSize);
		fwrite(pDst, 1, outBufferPos, pOutf);
		if (res == SZ_OK)
		{
			if (status == LZMA_STATUS_FINISHED_WITH_MARK || (status == LZMA_STATUS_MAYBE_FINISHED_WITHOUT_MARK && outLeft == 0 && srcLeft == 0))
			{
				break;
			}
			if (outLeft <= 0 || (srcLeft == 0 && status == LZMA_STATUS_NEEDS_MORE_INPUT))
			{
				res = SZ_ERROR_DATA;
				break;
			}
		}
		else {
			break;
		}
	}
	LzmaDec_Free(&p, &alloc);
	fflush(pOutf);
	fclose(pf);
	fclose(pOutf);
	free(pSrc);
	free(pDst);
	totalOutSize = 0;
	return res;
}