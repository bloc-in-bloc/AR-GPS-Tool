package com.blocinbloc.bluetooth

import android.os.Build
import android.os.Looper
import java.util.concurrent.ExecutorService
import java.util.concurrent.Executors

object Executor {
    val ioExecutorService: ExecutorService = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
        Executors.newWorkStealingPool()
    } else {
        Executors.newCachedThreadPool()
    }
}

object Handler {
    val mainHandler = android.os.Handler(Looper.getMainLooper())
}

internal inline fun executeOnBackground(noinline runnable: () -> Unit) {
    Executor.ioExecutorService.submit(runnable)
}

internal inline fun executeOnUi(noinline runnable: () -> Unit) {
    Handler.mainHandler.post(runnable)
}