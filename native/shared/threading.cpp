#include <sched.h>

extern "C" {
    int SwitchToThread(){
        return sched_yield();
    }
}