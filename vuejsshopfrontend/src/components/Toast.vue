<template>
  <teleport to="body">
    <div class="fixed top-16 right-4 z-[9999] flex flex-col items-end">
      <transition-group name="toast" tag="div">
        <div
          v-for="t in toasts"
          :key="t.id"
          class="mb-2 w-80 p-4 rounded-lg shadow-lg flex items-start border border-[#3a3a3a] bg-[#1a1a1a]"
        >
          <div
            class="flex-1 text-sm"
            :class="t.type === 'success' ? 'text-[#b5d89c]' : 'text-[#f28b82]'"
          >
            {{ t.message }}
          </div>
          <button
            @click="$emit('close', t.id)"
            class="ml-3 text-[#888] hover:text-[#ccc] transition"
            aria-label="Close"
          >
            ×
          </button>
        </div>
      </transition-group>
    </div>
  </teleport>
</template>

<script setup>
defineProps({ toasts: Array });
defineEmits(['close']);
</script>

<style>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
.toast-enter-to,
.toast-leave-from {
  opacity: 1;
  transform: translateY(0);
}
</style>
